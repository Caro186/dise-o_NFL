using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.DTOs;
using Backend.Configuration;

namespace NFLFantasyAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar equipos fantasy de usuarios
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EquipoFantasyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EquipoFantasyController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly FileServerSettings _fileServerSettings;

        public EquipoFantasyController(
            ApplicationDbContext context,
            ILogger<EquipoFantasyController> logger,
            IWebHostEnvironment environment,
            IOptions<FileServerSettings> fileServerSettings)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
            _fileServerSettings = fileServerSettings.Value;
        }

        /// <summary>
        /// Obtiene todos los equipos fantasy
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<EquipoFantasyResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllEquiposFantasy()
        {
            try
            {
                var equipos = await _context.EquiposFantasy
                    .Include(e => e.Usuario)
                    .Include(e => e.Liga)
                    .Select(e => new EquipoFantasyResponseDto
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        UsuarioId = e.UsuarioId,
                        NombrePropietario = e.Usuario!.NombreCompleto,
                        LigaId = e.LigaId,
                        NombreLiga = e.Liga != null ? e.Liga.NombreLiga : null,
                        ImagenUrl = e.ImagenUrl,
                        FechaCreacion = e.FechaCreacion,
                        Estado = e.Estado
                    })
                    .ToListAsync();

                return Ok(equipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipos fantasy");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Obtiene un equipo fantasy por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EquipoFantasyResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetEquipoFantasy(int id)
        {
            try
            {
                var equipo = await _context.EquiposFantasy
                    .Include(e => e.Usuario)
                    .Include(e => e.Liga)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo fantasy no encontrado"
                    });
                }

                var response = new EquipoFantasyResponseDto
                {
                    Id = equipo.Id,
                    Nombre = equipo.Nombre,
                    UsuarioId = equipo.UsuarioId,
                    NombrePropietario = equipo.Usuario?.NombreCompleto,
                    LigaId = equipo.LigaId,
                    NombreLiga = equipo.Liga?.NombreLiga,
                    ImagenUrl = equipo.ImagenUrl,
                    FechaCreacion = equipo.FechaCreacion,
                    Estado = equipo.Estado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipo fantasy {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Obtiene todos los equipos fantasy de un usuario
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(List<EquipoFantasyResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetEquiposByUsuario(int usuarioId)
        {
            try
            {
                var equipos = await _context.EquiposFantasy
                    .Include(e => e.Usuario)
                    .Include(e => e.Liga)
                    .Where(e => e.UsuarioId == usuarioId)
                    .Select(e => new EquipoFantasyResponseDto
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        UsuarioId = e.UsuarioId,
                        NombrePropietario = e.Usuario!.NombreCompleto,
                        LigaId = e.LigaId,
                        NombreLiga = e.Liga != null ? e.Liga.NombreLiga : null,
                        ImagenUrl = e.ImagenUrl,
                        FechaCreacion = e.FechaCreacion,
                        Estado = e.Estado
                    })
                    .ToListAsync();

                return Ok(equipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipos del usuario {UsuarioId}", usuarioId);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Crea un nuevo equipo fantasy
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(EquipoFantasyResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateEquipoFantasy([FromBody] EquipoFantasyCreateDto equipoDto)
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

                // Verificar que el usuario existe
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == equipoDto.UsuarioId);
                if (!usuarioExiste)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Usuario no encontrado"
                    });
                }

                // Verificar que el nombre sea único para el usuario
                var nombreExiste = await _context.EquiposFantasy
                    .AnyAsync(e => e.Nombre == equipoDto.Nombre && e.UsuarioId == equipoDto.UsuarioId);

                if (nombreExiste)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Ya tienes un equipo con ese nombre"
                    });
                }

                var equipo = new EquipoFantasy
                {
                    Nombre = equipoDto.Nombre,
                    UsuarioId = equipoDto.UsuarioId,
                    LigaId = equipoDto.LigaId,
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Activo"
                };

                _context.EquiposFantasy.Add(equipo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipo fantasy creado: {Nombre} por usuario {UsuarioId}",
                    equipo.Nombre, equipo.UsuarioId);

                // Cargar relaciones para la respuesta
                await _context.Entry(equipo).Reference(e => e.Usuario).LoadAsync();
                if (equipo.LigaId.HasValue)
                {
                    await _context.Entry(equipo).Reference(e => e.Liga).LoadAsync();
                }

                var response = new EquipoFantasyResponseDto
                {
                    Id = equipo.Id,
                    Nombre = equipo.Nombre,
                    UsuarioId = equipo.UsuarioId,
                    NombrePropietario = equipo.Usuario?.NombreCompleto,
                    LigaId = equipo.LigaId,
                    NombreLiga = equipo.Liga?.NombreLiga,
                    ImagenUrl = equipo.ImagenUrl,
                    FechaCreacion = equipo.FechaCreacion,
                    Estado = equipo.Estado
                };

                return CreatedAtAction(nameof(GetEquipoFantasy), new { id = equipo.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear equipo fantasy");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Sube la imagen de un equipo fantasy
        /// </summary>
        [HttpPost("{id}/imagen")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UploadImagen(int id, IFormFile imagen)
        {
            try
            {
                var equipo = await _context.EquiposFantasy.FindAsync(id);
                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo fantasy no encontrado"
                    });
                }

                if (imagen == null || imagen.Length == 0)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "No se proporcionó ninguna imagen"
                    });
                }

                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(imagen.ContentType.ToLower()))
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Solo se permiten imágenes JPEG o PNG"
                    });
                }

                if (imagen.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "El tamaño máximo permitido es 5 MB"
                    });
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "equipos-fantasy");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extension = Path.GetExtension(imagen.FileName);
                var fileName = $"{id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                equipo.ImagenUrl = $"{_fileServerSettings.BaseUrl}/uploads/equipos-fantasy/{fileName}";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Imagen actualizada para equipo fantasy {Id}", equipo.Id);

                return Ok(new
                {
                    mensaje = "Imagen actualizada exitosamente",
                    imagenUrl = equipo.ImagenUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen del equipo fantasy {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Elimina un equipo fantasy
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteEquipoFantasy(int id)
        {
            try
            {
                var equipo = await _context.EquiposFantasy.FindAsync(id);
                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo fantasy no encontrado"
                    });
                }

                _context.EquiposFantasy.Remove(equipo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipo fantasy eliminado: {Id}", id);

                return Ok(new { mensaje = "Equipo fantasy eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar equipo fantasy {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }
    }
}