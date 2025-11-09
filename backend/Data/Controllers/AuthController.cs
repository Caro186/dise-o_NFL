using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.DTOs;
using Backend.Configuration;

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
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            ApplicationDbContext context, 
            ILogger<AuthController> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }

        /// <summary>
        /// Registra un nuevo usuario (siempre como "Usuario", nunca como "Admin")
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register([FromBody] RegistroDto registroDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos inválidos",
                        Errores = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Verificar si el email ya existe
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Email == registroDto.Email);
                if (usuarioExiste)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "El email ya está registrado"
                    });
                }

                // Hashear contraseña
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registroDto.Password);

                // Crear usuario (siempre con rol "Usuario")
                var usuario = new Usuario
                {
                    Email = registroDto.Email,
                    Password = passwordHash,
                    NombreCompleto = registroDto.NombreCompleto,
                    FechaRegistro = DateTime.UtcNow,
                    EstadoCuenta = "Activa",
                    IntentosFailidos = 0,
                    Rol = "Usuario"
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario registrado: {Email} con rol {Rol}", usuario.Email, usuario.Rol);

                return Created("", new
                {
                    mensaje = "Usuario registrado exitosamente",
                    usuario = new UsuarioDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        FechaRegistro = usuario.FechaRegistro,
                        Rol = usuario.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Inicia sesión de un usuario
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos inválidos",
                        Errores = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Buscar usuario por email
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (usuario == null)
                {
                    return Unauthorized(new ErrorResponseDto
                    {
                        Mensaje = "Credenciales inválidas"
                    });
                }

                // Verificar estado de la cuenta
                if (usuario.EstadoCuenta != "Activa")
                {
                    return Unauthorized(new ErrorResponseDto
                    {
                        Mensaje = "Cuenta bloqueada. Contacta al administrador."
                    });
                }

                // Verificar contraseña
                bool passwordValido = BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.Password);

                if (!passwordValido)
                {
                    // Incrementar intentos fallidos
                    usuario.IntentosFailidos++;
                    usuario.FechaUltimoIntentoFallido = DateTime.UtcNow;

                    if (usuario.IntentosFailidos >= 5)
                    {
                        usuario.EstadoCuenta = "Bloqueada";
                        usuario.FechaBloqueo = DateTime.UtcNow;
                        _logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Email}", usuario.Email);
                    }

                    await _context.SaveChangesAsync();

                    return Unauthorized(new ErrorResponseDto
                    {
                        Mensaje = "Credenciales inválidas"
                    });
                }

                // Login exitoso - resetear intentos fallidos
                usuario.IntentosFailidos = 0;
                usuario.FechaUltimoIntentoFallido = null;
                usuario.UltimaActividad = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generar token JWT
                var token = GenerateJwtToken(usuario);
                var tokenExpiracion = DateTime.UtcNow.AddHours(12);

                _logger.LogInformation("Login exitoso: {Email} con rol {Rol}", usuario.Email, usuario.Rol);

                return Ok(new LoginResponseDto
                {
                    Status = "ok",
                    Token = token,
                    TokenExpiracion = tokenExpiracion.ToString("o"),
                    Usuario = new UsuarioDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        FechaRegistro = usuario.FechaRegistro,
                        Rol = usuario.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Genera un token JWT para el usuario
        /// </summary>
        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Desbloquea una cuenta de usuario
        /// </summary>
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

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
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

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
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

        /// <summary>
        /// Elimina un usuario
        /// </summary>
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