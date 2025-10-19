using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using BCrypt.Net;
using NFLFantasyAPI.DTOs;

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

        /// <summary>
        /// Constructor del controlador de autenticación
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        /// <param name="logger">Logger para registrar eventos</param>
        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="registroDto">Datos del usuario a registrar</param>
        /// <returns>Información del usuario registrado</returns>
        /// <response code="200">Usuario registrado exitosamente</response>
        /// <response code="400">Datos de registro inválidos o email ya existe</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register(RegistroDto registroDto)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos de registro inválidos",
                        Errores = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Verificar si el email ya existe
                if (await _context.Usuarios.AnyAsync(u => u.Email == registroDto.Email))
                {
                    _logger.LogWarning("Intento de registro con email duplicado: {Email}", registroDto.Email);
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "El email ya está registrado"
                    });
                }

                // Crear usuario con contraseña encriptada
                var usuario = new Usuario
                {
                    Email = registroDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registroDto.Password),
                    NombreCompleto = registroDto.NombreCompleto,
                    FechaRegistro = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario registrado exitosamente: {Email}", usuario.Email);

                // Retornar respuesta sin contraseña
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
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al registrar usuario"
                });
            }
        }

        /// <summary>
        /// Autentica un usuario en el sistema
        /// </summary>
        /// <param name="loginDto">Credenciales del usuario</param>
        /// <returns>Información del usuario autenticado</returns>
        /// <response code="200">Login exitoso</response>
        /// <response code="401">Credenciales inválidas</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos de login inválidos",
                        Errores = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Buscar usuario por email
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Intento de login con email no existente: {Email}", loginDto.Email);
                    return Unauthorized(new ErrorResponseDto
                    {
                        Mensaje = "Email o contraseña incorrectos"
                    });
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.Password))
                {
                    _logger.LogWarning("Intento de login con contraseña incorrecta para: {Email}", loginDto.Email);
                    return Unauthorized(new ErrorResponseDto
                    {
                        Mensaje = "Email o contraseña incorrectos"
                    });
                }

                _logger.LogInformation("Login exitoso para usuario: {Email}", usuario.Email);

                // Retornar respuesta sin contraseña
                return Ok(new LoginResponseDto
                {
                    Status = "ok",
                    Usuario = new UsuarioResponseDto
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
                _logger.LogError(ex, "Error al procesar login");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al procesar login"
                });
            }
        }

        /// <summary>
        /// Obtiene la lista de todos los usuarios registrados
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet("usuarios")]
        [ProducesResponseType(typeof(List<UsuarioResponseDto>), StatusCodes.Status200OK)]
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
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al obtener usuarios"
                });
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Información del usuario</returns>
        [HttpGet("usuario/{id}")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Usuario no encontrado"
                    });
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
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al obtener usuario"
                });
            }
        }

        /// <summary>
        /// Elimina un usuario del sistema
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("usuario/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Usuario no encontrado"
                    });
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario eliminado: {Id}", id);

                return Ok(new { mensaje = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al eliminar usuario"
                });
            }
        }
    }
}