using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.DTOs;
using Backend.Configuration;
using BCrypt.Net;

namespace NFLFantasyAPI.Controllers
{
    /// <summary>
    /// Controlador para gestión de ligas
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LigaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LigaController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly FileServerSettings _fileServerSettings;

        public LigaController(
            ApplicationDbContext context,
            ILogger<LigaController> logger,
            IWebHostEnvironment environment,
            IOptions<FileServerSettings> fileServerSettings)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
            _fileServerSettings = fileServerSettings.Value;
        }

        /// <summary>
        /// Obtiene todas las ligas
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<LigaResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllLigas()
        {
            try
            {
                var ligas = await _context.Ligas
                    .Include(l => l.Comisionado)
                    .Include(l => l.Temporada)
                    .Select(l => new LigaResponseDto
                    {
                        IdLiga = l.IdLiga,
                        ImagenUrl = l.ImagenUrl,
                        NombreLiga = l.NombreLiga,
                        Descripcion = l.Descripcion,
                        IdTemporada = l.IdTemporada,
                        NombreTemporada = l.Temporada != null ? l.Temporada.Nombre.ToString() : null,
                        Estado = l.Estado,
                        CuposTotales = l.CuposTotales,
                        CuposOcupados = l.CuposOcupados,
                        FechaCreacion = l.FechaCreacion,
                        FechaInicio = l.FechaInicio,
                        FechaFin = l.FechaFin,
                        ComisionadoId = l.ComisionadoId,
                        NombreComisionado = l.Comisionado != null ? l.Comisionado.NombreCompleto : null,
                        FormatoPosiciones = l.FormatoPosiciones,
                        EsquemaPuntos = l.EsquemaPuntos,
                        ConfigPlayoffs = l.ConfigPlayoffs,
                        PermitirDecimales = l.PermitirDecimales
                    })
                    .ToListAsync();

                return Ok(ligas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ligas");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor al obtener ligas"
                });
            }
        }



                /// <summary>
        /// Permite a un usuario unirse a una liga existente
        /// </summary>
        [HttpPost("unirse")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UnirseLiga([FromBody] UnirseLigaDto dto)
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

                // Verificar que la liga existe
                var liga = await _context.Ligas.FindAsync(dto.LigaId);
                if (liga == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Liga no encontrada"
                    });
                }

                // Verificar que el usuario existe
                var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
                if (usuario == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Usuario no encontrado"
                    });
                }

                // Verificar que el equipo existe y pertenece al usuario
                var equipo = await _context.EquiposFantasy.FindAsync(dto.EquipoId);
                if (equipo == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Equipo no encontrado"
                    });
                }

                if (equipo.UsuarioId != dto.UsuarioId)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "El equipo no pertenece al usuario"
                    });
                }

                // Verificar la contraseña de la liga
                bool passwordValido = BCrypt.Net.BCrypt.Verify(dto.Password, liga.PasswordHash);
                if (!passwordValido)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Contraseña incorrecta"
                    });
                }

                // Verificar que la liga no esté llena
                if (liga.CuposOcupados >= liga.CuposTotales)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "La liga está llena"
                    });
                }

                // Verificar que el usuario no esté ya en la liga
                var yaEstaEnLiga = await _context.EquiposLigas
                    .AnyAsync(el => el.IdLiga == dto.LigaId && el.IdEquipo == dto.EquipoId);

                if (yaEstaEnLiga)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Ya estás en esta liga con este equipo"
                    });
                }

                // Verificar que el alias sea único en la liga
                var aliasExiste = await _context.EquiposLigas
                    .AnyAsync(el => el.IdLiga == dto.LigaId && el.Alias == dto.Alias);

                if (aliasExiste)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "El alias ya está en uso en esta liga"
                    });
                }

                // Crear la relación equipo-liga
                var equipoLiga = new EquipoLiga
                {
                    IdEquipo = dto.EquipoId,
                    IdLiga = dto.LigaId,
                    Alias = dto.Alias,
                    FechaUnion = DateTime.UtcNow,
                    EsComisionado = false
                };

                _context.EquiposLigas.Add(equipoLiga);

                // Actualizar cupos ocupados
                liga.CuposOcupados++;

                // Actualizar la liga del equipo fantasy
                equipo.LigaId = dto.LigaId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario {UsuarioId} se unió a la liga {LigaId} con equipo {EquipoId}",
                    dto.UsuarioId, dto.LigaId, dto.EquipoId);

                return Ok(new
                {
                    mensaje = $"Te has unido exitosamente a la liga '{liga.NombreLiga}'",
                    ligaId = liga.IdLiga,
                    nombreLiga = liga.NombreLiga,
                    equipoId = dto.EquipoId,
                    alias = dto.Alias
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unirse a la liga");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }



        /// <summary>
        /// Obtiene una liga por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LigaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetLiga(int id)
        {
            try
            {
                var liga = await _context.Ligas
                    .Include(l => l.Comisionado)
                    .Include(l => l.Temporada)
                    .FirstOrDefaultAsync(l => l.IdLiga == id);

                if (liga == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Liga no encontrada"
                    });
                }

                var response = new LigaResponseDto
                {
                    IdLiga = liga.IdLiga,
                    ImagenUrl = liga.ImagenUrl,
                    NombreLiga = liga.NombreLiga,
                    Descripcion = liga.Descripcion,
                    IdTemporada = liga.IdTemporada,
                    NombreTemporada = liga.Temporada?.Nombre.ToString(),
                    Estado = liga.Estado,
                    CuposTotales = liga.CuposTotales,
                    CuposOcupados = liga.CuposOcupados,
                    FechaCreacion = liga.FechaCreacion,
                    FechaInicio = liga.FechaInicio,
                    FechaFin = liga.FechaFin,
                    ComisionadoId = liga.ComisionadoId,
                    NombreComisionado = liga.Comisionado?.NombreCompleto,
                    FormatoPosiciones = liga.FormatoPosiciones,
                    EsquemaPuntos = liga.EsquemaPuntos,
                    ConfigPlayoffs = liga.ConfigPlayoffs,
                    PermitirDecimales = liga.PermitirDecimales
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener liga {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Obtiene todas las ligas donde un usuario es comisionado
        /// </summary>
        [HttpGet("comisionado/{usuarioId}")]
        [ProducesResponseType(typeof(List<LigaResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetLigasPorComisionado(int usuarioId)
        {
            try
            {
                var ligas = await _context.Ligas
                    .Include(l => l.Comisionado)
                    .Include(l => l.Temporada)
                    .Where(l => l.ComisionadoId == usuarioId)
                    .Select(l => new LigaResponseDto
                    {
                        IdLiga = l.IdLiga,
                        ImagenUrl = l.ImagenUrl,
                        NombreLiga = l.NombreLiga,
                        Descripcion = l.Descripcion,
                        IdTemporada = l.IdTemporada,
                        NombreTemporada = l.Temporada != null ? l.Temporada.Nombre.ToString() : null,
                        Estado = l.Estado,
                        CuposTotales = l.CuposTotales,
                        CuposOcupados = l.CuposOcupados,
                        FechaCreacion = l.FechaCreacion,
                        FechaInicio = l.FechaInicio,
                        FechaFin = l.FechaFin,
                        ComisionadoId = l.ComisionadoId,
                        NombreComisionado = l.Comisionado != null ? l.Comisionado.NombreCompleto : null,
                        FormatoPosiciones = l.FormatoPosiciones,
                        EsquemaPuntos = l.EsquemaPuntos,
                        ConfigPlayoffs = l.ConfigPlayoffs,
                        PermitirDecimales = l.PermitirDecimales
                    })
                    .ToListAsync();

                return Ok(ligas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ligas del comisionado {UsuarioId}", usuarioId);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Crea una nueva liga
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(LigaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateLiga([FromBody] LigaCreateDto ligaDto)
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

                // Verificar que el usuario comisionado existe
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == ligaDto.ComisionadoId);
                if (!usuarioExiste)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Usuario comisionado no encontrado"
                    });
                }

                // Verificar que la temporada existe
                var temporadaExiste = await _context.Temporadas.AnyAsync(t => t.Id == ligaDto.IdTemporada);
                if (!temporadaExiste)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Temporada no encontrada"
                    });
                }

                // Verificar que el nombre de la liga sea único
                var nombreExiste = await _context.Ligas
                    .AnyAsync(l => l.NombreLiga == ligaDto.NombreLiga);
                if (nombreExiste)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Ya existe una liga con ese nombre"
                    });
                }

                // Hashear la contraseña
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(ligaDto.PasswordHash);

                var liga = new Liga
                {
                    NombreLiga = ligaDto.NombreLiga,
                    Descripcion = ligaDto.Descripcion,
                    PasswordHash = passwordHash,
                    IdTemporada = ligaDto.IdTemporada,
                    Estado = "Pre-Draft",
                    CuposTotales = ligaDto.CuposTotales,
                    CuposOcupados = 1, // El comisionado cuenta como 1
                    FechaCreacion = DateTime.UtcNow,
                    ComisionadoId = ligaDto.ComisionadoId,
                    FormatoPosiciones = ligaDto.FormatoPosiciones,
                    EsquemaPuntos = ligaDto.EsquemaPuntos,
                    ConfigPlayoffs = ligaDto.ConfigPlayoffs,
                    PermitirDecimales = ligaDto.PermitirDecimales
                };

                _context.Ligas.Add(liga);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Liga creada: {NombreLiga} por usuario {ComisionadoId}",
                    liga.NombreLiga, liga.ComisionadoId);

                // Cargar relaciones para la respuesta
                await _context.Entry(liga).Reference(l => l.Comisionado).LoadAsync();
                await _context.Entry(liga).Reference(l => l.Temporada).LoadAsync();

                var response = new LigaResponseDto
                {
                    IdLiga = liga.IdLiga,
                    ImagenUrl = liga.ImagenUrl,
                    NombreLiga = liga.NombreLiga,
                    Descripcion = liga.Descripcion,
                    IdTemporada = liga.IdTemporada,
                    NombreTemporada = liga.Temporada?.Nombre.ToString(),
                    Estado = liga.Estado,
                    CuposTotales = liga.CuposTotales,
                    CuposOcupados = liga.CuposOcupados,
                    FechaCreacion = liga.FechaCreacion,
                    FechaInicio = liga.FechaInicio,
                    FechaFin = liga.FechaFin,
                    ComisionadoId = liga.ComisionadoId,
                    NombreComisionado = liga.Comisionado?.NombreCompleto,
                    FormatoPosiciones = liga.FormatoPosiciones,
                    EsquemaPuntos = liga.EsquemaPuntos,
                    ConfigPlayoffs = liga.ConfigPlayoffs,
                    PermitirDecimales = liga.PermitirDecimales
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear liga");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Actualiza una liga existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(LigaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateLiga(int id, [FromBody] LigaCreateDto ligaDto)
        {
            try
            {
                var liga = await _context.Ligas.FindAsync(id);
                if (liga == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Liga no encontrada"
                    });
                }

                // Verificar que el nombre sea único (excepto la liga actual)
                var nombreExiste = await _context.Ligas
                    .AnyAsync(l => l.NombreLiga == ligaDto.NombreLiga && l.IdLiga != id);
                if (nombreExiste)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Ya existe otra liga con ese nombre"
                    });
                }

                // Actualizar campos
                liga.NombreLiga = ligaDto.NombreLiga;
                liga.Descripcion = ligaDto.Descripcion;
                
                // Solo actualizar password si se proporciona uno nuevo
                if (!string.IsNullOrWhiteSpace(ligaDto.PasswordHash))
                {
                    liga.PasswordHash = BCrypt.Net.BCrypt.HashPassword(ligaDto.PasswordHash);
                }

                liga.CuposTotales = ligaDto.CuposTotales;
                liga.FormatoPosiciones = ligaDto.FormatoPosiciones;
                liga.EsquemaPuntos = ligaDto.EsquemaPuntos;
                liga.ConfigPlayoffs = ligaDto.ConfigPlayoffs;
                liga.PermitirDecimales = ligaDto.PermitirDecimales;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Liga actualizada: {IdLiga}", id);

                // Cargar relaciones para la respuesta
                await _context.Entry(liga).Reference(l => l.Comisionado).LoadAsync();
                await _context.Entry(liga).Reference(l => l.Temporada).LoadAsync();

                var response = new LigaResponseDto
                {
                    IdLiga = liga.IdLiga,
                    ImagenUrl = liga.ImagenUrl,
                    NombreLiga = liga.NombreLiga,
                    Descripcion = liga.Descripcion,
                    IdTemporada = liga.IdTemporada,
                    NombreTemporada = liga.Temporada?.Nombre.ToString(),
                    Estado = liga.Estado,
                    CuposTotales = liga.CuposTotales,
                    CuposOcupados = liga.CuposOcupados,
                    FechaCreacion = liga.FechaCreacion,
                    FechaInicio = liga.FechaInicio,
                    FechaFin = liga.FechaFin,
                    ComisionadoId = liga.ComisionadoId,
                    NombreComisionado = liga.Comisionado?.NombreCompleto,
                    FormatoPosiciones = liga.FormatoPosiciones,
                    EsquemaPuntos = liga.EsquemaPuntos,
                    ConfigPlayoffs = liga.ConfigPlayoffs,
                    PermitirDecimales = liga.PermitirDecimales
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar liga {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Sube la imagen de una liga
        /// </summary>
        [HttpPost("{id}/imagen")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UploadImagen(int id, IFormFile imagen)
        {
            try
            {
                var liga = await _context.Ligas.FindAsync(id);
                if (liga == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Liga no encontrada"
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
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ligas");
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
                liga.ImagenUrl = $"{_fileServerSettings.BaseUrl}/uploads/ligas/{fileName}";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Imagen actualizada para liga {Id}", liga.IdLiga);

                return Ok(new
                {
                    mensaje = "Imagen actualizada exitosamente",
                    imagenUrl = liga.ImagenUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen de la liga {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Elimina una liga
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteLiga(int id)
        {
            try
            {
                var liga = await _context.Ligas.FindAsync(id);
                if (liga == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Liga no encontrada"
                    });
                }

                _context.Ligas.Remove(liga);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Liga eliminada: {Id}", id);

                return Ok(new { mensaje = "Liga eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar liga {Id}", id);
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error interno del servidor"
                });
            }
        }
    }
}