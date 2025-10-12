using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;

namespace NFLFantasyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<Usuario>> Register(RegistroDto registroDto)
        {
            // Verificar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == registroDto.Email))
            {
                return BadRequest(new { mensaje = "El email ya está registrado" });
            }

            var usuario = new Usuario
            {
                Email = registroDto.Email,
                Password = registroDto.Password, // En producción usar hash
                NombreCompleto = registroDto.NombreCompleto,
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { 
                mensaje = "Usuario registrado exitosamente",
                usuario = new {
                    usuario.Id,
                    usuario.Email,
                    usuario.NombreCompleto
                }
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos" });
            }

            return Ok(new { 
                mensaje = "Login exitoso",
                usuario = new {
                    usuario.Id,
                    usuario.Email,
                    usuario.NombreCompleto,
                    usuario.FechaRegistro
                }
            });
        }

        // GET: api/Auth/usuarios
        [HttpGet("usuarios")]
        public async Task<ActionResult<List<Usuario>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new {
                    u.Id,
                    u.Email,
                    u.NombreCompleto,
                    u.FechaRegistro
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/Auth/usuario/{id}
        [HttpGet("usuario/{id}")]
        public async Task<ActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { mensaje = "Usuario no encontrado" });
            }

            return Ok(new {
                usuario.Id,
                usuario.Email,
                usuario.NombreCompleto,
                usuario.FechaRegistro
            });
        }

        // DELETE: api/Auth/usuario/{id}
        [HttpDelete("usuario/{id}")]
        public async Task<ActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { mensaje = "Usuario no encontrado" });
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario eliminado exitosamente" });
        }
    }

    // DTOs (Data Transfer Objects)
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegistroDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
    }
}