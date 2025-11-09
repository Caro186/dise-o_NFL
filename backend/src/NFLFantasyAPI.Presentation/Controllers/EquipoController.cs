using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.DTOs;

namespace NFLFantasyAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar equipos de fantasy
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EquipoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EquipoController> _logger;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Constructor del controlador de equipos
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        /// <param name="logger">Logger para registrar eventos</param>
        /// <param name="environment">Entorno de hosting para manejo de archivos</param>
        public EquipoController(
            ApplicationDbContext context, 
            ILogger<EquipoController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Crea un nuevo equipo de fantasy
        /// </summary>
        /// <param name="equipoDto">Datos del equipo a crear</param>
        /// <returns>Información del equipo creado</returns>
        /// <response code="201">Equipo creado exitosamente</response>
        /// <response code="400">Datos de equipo inválidos</response>
        /// <response code="404">Usuario no encontrado</response>
        [HttpPost]
        [ProducesResponseType(typeof(EquipoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CreateEquipo([FromBody] EquipoCreateDto equipoDto)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos de equipo inválidos",
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

                // Verificar unicidad del nombre de equipo (opcional: por usuario o global)
                var nombreExiste = await _context.Equipos
                    .AnyAsync(e => e.Nombre == equipoDto.Nombre && e.UsuarioId == equipoDto.UsuarioId);
                
                if (nombreExiste)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Ya existe un equipo con ese nombre para este usuario"
                    });
                }

                // Crear equipo
                var equipo = new Equipo
                {
                    Nombre = equipoDto.Nombre,
                    UsuarioId = equipoDto.UsuarioId,
                    Liga = equipoDto.Liga,
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Activo"
                };

                _context.Equipos.Add(equipo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipo creado exitosamente: {Nombre} por usuario {UsuarioId}", 
                    equipo.Nombre, equipo.UsuarioId);

                // Cargar información del usuario para la respuesta
                var usuario = await _context.Usuarios.FindAsync(equipoDto.UsuarioId);

                var response = new EquipoResponseDto
                {
                    Id = equipo.Id,
                    Nombre = equipo.Nombre,
                    ImagenUrl = equipo.ImagenUrl,
                    FechaCreacion = equipo.FechaCreacion,
                    UsuarioId = equipo.UsuarioId,
                    NombrePropietario = usuario?.NombreCompleto,
                    Estado = equipo.Estado,
                    Liga = equipo.Liga
                };

                return CreatedAtAction(nameof(GetEquipo), new { id = equipo.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear equipo");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al crear equipo"
                });
            }
        }

        /// <summary>
        /// Obtiene un equipo por su ID
        /// </summary>
        /// <param name="id">ID del equipo</param>
        /// <returns>Información del equipo</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EquipoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetEquipo(int id)
        {
            try
            {
                var equipo = await _context.Equipos
                    .Include(e => e.Usuario)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo no encontrado"
                    });
                }

                var response = new EquipoResponseDto
                {
                    Id = equipo.Id,
                    Nombre = equipo.Nombre,
                    ImagenUrl = equipo.ImagenUrl,
                    FechaCreacion = equipo.FechaCreacion,
                    UsuarioId = equipo.UsuarioId,
                    NombrePropietario = equipo.Usuario?.NombreCompleto,
                    Estado = equipo.Estado,
                    Liga = equipo.Liga
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipo {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al obtener equipo"
                });
            }
        }

        /// <summary>
        /// Obtiene todos los equipos de un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Lista de equipos del usuario</returns>
        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(List<EquipoResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetEquiposByUsuario(int usuarioId)
        {
            try
            {
                var equipos = await _context.Equipos
                    .Include(e => e.Usuario)
                    .Where(e => e.UsuarioId == usuarioId)
                    .Select(e => new EquipoResponseDto
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        ImagenUrl = e.ImagenUrl,
                        FechaCreacion = e.FechaCreacion,
                        UsuarioId = e.UsuarioId,
                        NombrePropietario = e.Usuario!.NombreCompleto,
                        Estado = e.Estado,
                        Liga = e.Liga
                    })
                    .ToListAsync();

                return Ok(equipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipos del usuario {UsuarioId}", usuarioId);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al obtener equipos"
                });
            }
        }

        /// <summary>
        /// Obtiene todos los equipos registrados
        /// </summary>
        /// <returns>Lista de todos los equipos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<EquipoResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllEquipos()
        {
            try
            {
                var equipos = await _context.Equipos
                    .Include(e => e.Usuario)
                    .Select(e => new EquipoResponseDto
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        ImagenUrl = e.ImagenUrl,
                        FechaCreacion = e.FechaCreacion,
                        UsuarioId = e.UsuarioId,
                        NombrePropietario = e.Usuario!.NombreCompleto,
                        Estado = e.Estado,
                        Liga = e.Liga
                    })
                    .ToListAsync();

                return Ok(equipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los equipos");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al obtener equipos"
                });
            }
        }

        /// <summary>
        /// Actualiza la imagen de un equipo
        /// </summary>
        /// <param name="id">ID del equipo</param>
        /// <param name="imagen">Archivo de imagen</param>
        /// <returns>URL de la imagen actualizada</returns>
        [HttpPost("{id}/imagen")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UploadImagen(int id, IFormFile imagen)
        {
            try
            {
                // Validar que existe el equipo
                var equipo = await _context.Equipos.FindAsync(id);
                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo no encontrado"
                    });
                }

                // Validar archivo
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
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "equipos");
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
                equipo.ImagenUrl = $"/uploads/equipos/{fileName}";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Imagen actualizada para equipo {Id}: {ImagenUrl}", 
                    equipo.Id, equipo.ImagenUrl);

                return Ok(new
                {
                    mensaje = "Imagen actualizada exitosamente",
                    imagenUrl = equipo.ImagenUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen para equipo {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al subir imagen"
                });
            }
        }

        /// <summary>
        /// Elimina un equipo
        /// </summary>
        /// <param name="id">ID del equipo a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteEquipo(int id)
        {
            try
            {
                var equipo = await _context.Equipos.FindAsync(id);
                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo no encontrado"
                    });
                }

                // Eliminar imagen física si existe
                if (!string.IsNullOrEmpty(equipo.ImagenUrl))
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, equipo.ImagenUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Equipos.Remove(equipo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipo eliminado: {Id}", id);

                return Ok(new { mensaje = "Equipo eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar equipo {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al eliminar equipo"
                });
            }
        }
    }
}