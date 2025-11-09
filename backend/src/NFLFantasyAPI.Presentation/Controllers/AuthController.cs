using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using BCrypt.Net;
using NFLFantasyAPI.DTOs;
using NFLFantasyAPI.Services;

namespace NFLFantasyAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar autenticación y usuarios
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IJwtService _jwtService;

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger, IJwtService jwtService)
        {
            _context = context;
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegistroDto registroDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos de registro inválidos",
                        Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                if (await _context.Usuarios.AnyAsync(u => u.Email == registroDto.Email))
                {
                    _logger.LogWarning("Intento de registro con email duplicado: {Email}", registroDto.Email);
                    return BadRequest(new ErrorResponseDto { Mensaje = "El email ya está registrado" });
                }

                var usuario = new Usuario
                {
                    Email = registroDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registroDto.Password),
                    NombreCompleto = registroDto.NombreCompleto,
                    FechaRegistro = DateTime.UtcNow,
                    IntentosFailidos = 0,
                    EstadoCuenta = "Activa"
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuario registrado exitosamente: {Email}", usuario.Email);

                return Ok(new
                {
                    mensaje = "Usuario registrado exitosamente",
                    usuario = new UsuarioResponseDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        FechaRegistro = usuario.FechaRegistro
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error interno del servidor al registrar usuario" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos de login inválidos",
                        Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Intento de login con email no existente: {Email}", loginDto.Email);
                    return Unauthorized(new ErrorResponseDto { Mensaje = "Email o contraseña incorrectos" });
                }

                if (usuario.EstadoCuenta == "Bloqueada")
                {
                    _logger.LogWarning("Intento de login con cuenta bloqueada: {Email}", loginDto.Email);
                    return StatusCode(403, new ErrorResponseDto
                    {
                        Mensaje = "Tu cuenta ha sido bloqueada por múltiples intentos fallidos de inicio de sesión. Por favor, contacta al administrador."
                    });
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.Password))
                {
                    usuario.IntentosFailidos++;
                    usuario.FechaUltimoIntentoFallido = DateTime.UtcNow;

                    if (usuario.IntentosFailidos >= 5)
                    {
                        usuario.EstadoCuenta = "Bloqueada";
                        usuario.FechaBloqueo = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        _logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Email}", loginDto.Email);
                        return StatusCode(403, new ErrorResponseDto
                        {
                            Mensaje = "Tu cuenta ha sido bloqueada por múltiples intentos fallidos de inicio de sesión. Por favor, contacta al administrador."
                        });
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogWarning("Intento de login con contraseña incorrecta para: {Email}. Intentos: {Intentos}", loginDto.Email, usuario.IntentosFailidos);
                    return Unauthorized(new ErrorResponseDto { Mensaje = "Email o contraseña incorrectos" });
                }

                usuario.IntentosFailidos = 0;
                usuario.FechaUltimoIntentoFallido = null;
                usuario.UltimaActividad = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(usuario);
                var tokenExpiracion = DateTime.UtcNow.AddHours(12);
                _logger.LogInformation("Login exitoso para usuario: {Email}", usuario.Email);

                return Ok(new LoginResponseDto
                {
                    Status = "ok",
                    Usuario = new UsuarioResponseDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        FechaRegistro = usuario.FechaRegistro
                    },
                    Token = token,
                    TokenExpiracion = tokenExpiracion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar login");
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error interno del servidor al procesar login" });
            }
        }

        [HttpPost("desbloquear")]
        public async Task<ActionResult> DesbloquearCuenta([FromBody] DesbloquearCuentaDto dto)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (usuario == null)
                {
                    return NotFound(new ErrorResponseDto { Mensaje = "Usuario no encontrado" });
                }

                usuario.EstadoCuenta = "Activa";
                usuario.IntentosFailidos = 0;
                usuario.FechaUltimoIntentoFallido = null;
                usuario.FechaBloqueo = null;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cuenta desbloqueada: {Email}", dto.Email);

                return Ok(new { mensaje = "Cuenta desbloqueada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desbloquear cuenta {Email}", dto.Email);
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error interno del servidor al desbloquear cuenta" });
            }
        }

        [HttpGet("usuarios")]
        public async Task<ActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Select(u => new UsuarioResponseDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        NombreCompleto = u.NombreCompleto,
                        FechaRegistro = u.FechaRegistro
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error interno del servidor al obtener usuarios" });
            }
        }

        [HttpGet("usuario/{id}")]
        public async Task<ActionResult> GetUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound(new ErrorResponseDto { Mensaje = "Usuario no encontrado" });
                }

                return Ok(new UsuarioResponseDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    FechaRegistro = usuario.FechaRegistro
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {Id}", id);
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error interno del servidor al obtener usuario" });
            }
        }

        [HttpDelete("usuario/{id}")]
        public async Task<ActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound(new ErrorResponseDto { Mensaje = "Usuario no encontrado" });
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuario eliminado: {Id}", id);

                return Ok(new { mensaje = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error interno del servidor al eliminar usuario" });
            }
        }
    }
}