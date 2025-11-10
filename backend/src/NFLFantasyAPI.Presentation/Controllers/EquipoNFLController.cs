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
    /// Controlador para gestionar equipos reales de la NFL
    /// Solo accesible por administradores
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EquipoNFLController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EquipoNFLController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly FileServerSettings _fileServerSettings;

        public EquipoNFLController(
            ApplicationDbContext context,
            ILogger<EquipoNFLController> logger,
            IWebHostEnvironment environment,
            IOptions<FileServerSettings> fileServerSettings)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
            _fileServerSettings = fileServerSettings.Value;
        }

        /// <summary>
        /// Obtiene todos los equipos NFL
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<EquipoNFLResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllEquiposNFL()
        {
            try
            {
                var equipos = await _context.EquiposNFL
                    .Select(e => new EquipoNFLResponseDto
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        Ciudad = e.Ciudad,
                        ImagenUrl = e.ImagenUrl,
                        FechaCreacion = e.FechaCreacion,
                        Estado = e.Estado
                    })
                    .ToListAsync();

                return Ok(equipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipos NFL");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al obtener equipos NFL"
                });
            }
        }

        /// <summary>
        /// Obtiene un equipo NFL por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EquipoNFLResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetEquipoNFL(int id)
        {
            try
            {
                var equipo = await _context.EquiposNFL.FindAsync(id);

                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo NFL no encontrado"
                    });
                }

                var response = new EquipoNFLResponseDto
                {
                    Id = equipo.Id,
                    Nombre = equipo.Nombre,
                    Ciudad = equipo.Ciudad,
                    ImagenUrl = equipo.ImagenUrl,
                    FechaCreacion = equipo.FechaCreacion,
                    Estado = equipo.Estado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipo NFL {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Crea un nuevo equipo NFL (solo administradores)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(EquipoNFLResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateEquipoNFL([FromBody] EquipoNFLCreateDto equipoDto)
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

                // Verificar que no exista un equipo con el mismo nombre
                var existeEquipo = await _context.EquiposNFL
                    .AnyAsync(e => e.Nombre == equipoDto.Nombre);

                if (existeEquipo)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Ya existe un equipo NFL con ese nombre"
                    });
                }

                var equipo = new EquipoNFL
                {
                    Nombre = equipoDto.Nombre,
                    Ciudad = equipoDto.Ciudad,
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Activo"
                };

                _context.EquiposNFL.Add(equipo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipo NFL creado: {Nombre}", equipo.Nombre);

                var response = new EquipoNFLResponseDto
                {
                    Id = equipo.Id,
                    Nombre = equipo.Nombre,
                    Ciudad = equipo.Ciudad,
                    ImagenUrl = equipo.ImagenUrl,
                    FechaCreacion = equipo.FechaCreacion,
                    Estado = equipo.Estado
                };

                return CreatedAtAction(nameof(GetEquipoNFL), new { id = equipo.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear equipo NFL");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Sube la imagen de un equipo NFL
        /// </summary>
        [HttpPost("{id}/imagen")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UploadImagen(int id, IFormFile imagen)
        {
            try
            {
                var equipo = await _context.EquiposNFL.FindAsync(id);
                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo NFL no encontrado"
                    });
                }

                if (imagen == null || imagen.Length == 0)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "No se proporcionó ninguna imagen"
                    });
                }

                // Validar tipo de archivo
                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(imagen.ContentType.ToLower()))
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Solo se permiten imágenes JPEG o PNG"
                    });
                }

                // Validar tamaño (5 MB)
                if (imagen.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "El tamaño máximo permitido es 5 MB"
                    });
                }

                // Crear directorio si no existe
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "equipos-nfl");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generar nombre único para el archivo
                var extension = Path.GetExtension(imagen.FileName);
                var fileName = $"{id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Guardar archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                // Actualizar URL en base de datos
                equipo.ImagenUrl = $"{_fileServerSettings.BaseUrl}/uploads/equipos-nfl/{fileName}";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Imagen actualizada para equipo NFL {Id}", equipo.Id);

                return Ok(new
                {
                    mensaje = "Imagen actualizada exitosamente",
                    imagenUrl = equipo.ImagenUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen del equipo NFL {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Elimina un equipo NFL
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteEquipoNFL(int id)
        {
            try
            {
                var equipo = await _context.EquiposNFL.FindAsync(id);
                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo NFL no encontrado"
                    });
                }

                _context.EquiposNFL.Remove(equipo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipo NFL eliminado: {Id}", id);

                return Ok(new { mensaje = "Equipo NFL eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar equipo NFL {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }
    }
}