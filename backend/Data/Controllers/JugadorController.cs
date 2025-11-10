using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.DTOs;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.Services;

namespace NFLFantasyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JugadorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JugadorBatchService _batchService;
        private readonly ILogger<JugadorController> _logger;

        public JugadorController(
            ApplicationDbContext context,
            JugadorBatchService batchService,
            ILogger<JugadorController> logger)
        {
            _context = context;
            _batchService = batchService;
            _logger = logger;
        }

        // GET: api/Jugador
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JugadorListDto>>> GetJugadores()
        {
            var jugadores = await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .Select(j => new JugadorListDto
                {
                    Id = j.Id,
                    Nombre = j.Nombre,
                    Posicion = j.Posicion,
                    NombreEquipoNFL = j.EquipoNFL.Nombre,
                    ThumbnailUrl = j.ThumbnailUrl,
                    Estado = j.Estado
                })
                .ToListAsync();

            return Ok(jugadores);
        }

        // GET: api/Jugador/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JugadorResponseDto>> GetJugador(int id)
        {
            var jugador = await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .Where(j => j.Id == id)
                .Select(j => new JugadorResponseDto
                {
                    Id = j.Id,
                    Nombre = j.Nombre,
                    Posicion = j.Posicion,
                    EquipoNFLId = j.EquipoNFLId,
                    NombreEquipoNFL = j.EquipoNFL.Nombre,
                    CiudadEquipoNFL = j.EquipoNFL.Ciudad,
                    ImagenUrl = j.ImagenUrl,
                    ThumbnailUrl = j.ThumbnailUrl,
                    Estado = j.Estado,
                    FechaCreacion = j.FechaCreacion,
                    FechaActualizacion = j.FechaActualizacion
                })
                .FirstOrDefaultAsync();

            if (jugador == null)
            {
                return NotFound(new { mensaje = "Jugador no encontrado" });
            }

            return Ok(jugador);
        }

        // GET: api/Jugador/Equipo/5
        [HttpGet("Equipo/{equipoNFLId}")]
        public async Task<ActionResult<IEnumerable<JugadorListDto>>> GetJugadoresPorEquipo(int equipoNFLId)
        {
            var equipoExiste = await _context.EquiposNFL.AnyAsync(e => e.Id == equipoNFLId);
            if (!equipoExiste)
            {
                return NotFound(new { mensaje = "Equipo NFL no encontrado" });
            }

            var jugadores = await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .Where(j => j.EquipoNFLId == equipoNFLId)
                .Select(j => new JugadorListDto
                {
                    Id = j.Id,
                    Nombre = j.Nombre,
                    Posicion = j.Posicion,
                    NombreEquipoNFL = j.EquipoNFL.Nombre,
                    ThumbnailUrl = j.ThumbnailUrl,
                    Estado = j.Estado
                })
                .ToListAsync();

            return Ok(jugadores);
        }

        // POST: api/Jugador
        [HttpPost]
        public async Task<ActionResult<JugadorResponseDto>> CrearJugador(CrearJugadorDto dto)
        {
            // Validar que todos los campos requeridos estén presentes
            if (string.IsNullOrWhiteSpace(dto.Nombre) || 
                string.IsNullOrWhiteSpace(dto.Posicion) || 
                dto.EquipoNFLId <= 0)
            {
                return BadRequest(new { mensaje = "Todos los campos requeridos deben ser proporcionados" });
            }

            // Validar que el equipo NFL existe
            var equipoExiste = await _context.EquiposNFL.AnyAsync(e => e.Id == dto.EquipoNFLId);
            if (!equipoExiste)
            {
                return BadRequest(new { mensaje = "El equipo NFL especificado no existe" });
            }

            // Validar que no exista un jugador con el mismo nombre en el mismo equipo
            var jugadorExistente = await _context.Jugadores
                .AnyAsync(j => j.Nombre.ToLower() == dto.Nombre.ToLower() && 
                              j.EquipoNFLId == dto.EquipoNFLId);

            if (jugadorExistente)
            {
                return Conflict(new { mensaje = "Ya existe un jugador con ese nombre en el equipo NFL especificado" });
            }

            // Crear el jugador
            var jugador = new Jugador
            {
                Nombre = dto.Nombre.Trim(),
                Posicion = dto.Posicion.Trim(),
                EquipoNFLId = dto.EquipoNFLId,
                ImagenUrl = dto.ImagenUrl?.Trim(),
                ThumbnailUrl = dto.ThumbnailUrl?.Trim(),
                Estado = "Activo",
                FechaCreacion = DateTime.UtcNow
            };

            _context.Jugadores.Add(jugador);
            await _context.SaveChangesAsync();

            // Cargar el equipo NFL para la respuesta
            await _context.Entry(jugador)
                .Reference(j => j.EquipoNFL)
                .LoadAsync();

            var response = new JugadorResponseDto
            {
                Id = jugador.Id,
                Nombre = jugador.Nombre,
                Posicion = jugador.Posicion,
                EquipoNFLId = jugador.EquipoNFLId,
                NombreEquipoNFL = jugador.EquipoNFL.Nombre,
                CiudadEquipoNFL = jugador.EquipoNFL.Ciudad,
                ImagenUrl = jugador.ImagenUrl,
                ThumbnailUrl = jugador.ThumbnailUrl,
                Estado = jugador.Estado,
                FechaCreacion = jugador.FechaCreacion
            };

            return CreatedAtAction(nameof(GetJugador), new { id = jugador.Id }, response);
        }

        // ==========================================
        // NUEVO ENDPOINT BATCH
        // ==========================================
        
        /// <summary>
        /// Endpoint para crear múltiples jugadores desde un archivo JSON
        /// Implementa la lógica "todo-o-nada": si hay al menos un error, no se crea ningún jugador
        /// </summary>
        /// <param name="file">Archivo JSON con array de jugadores</param>
        /// <returns>Reporte completo de éxitos y errores</returns>
        [HttpPost("batch")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<JugadorBatchResultDto>> CrearJugadoresBatch(IFormFile file)
        {
            _logger.LogInformation("Iniciando procesamiento batch de jugadores");

            // Validar que se envió un archivo
            if (file == null)
            {
                return BadRequest(new JugadorBatchResultDto
                {
                    Exito = false,
                    Mensaje = "No se proporcionó ningún archivo",
                    Errores = new List<JugadorBatchErrorDto>
                    {
                        new JugadorBatchErrorDto { Error = "Archivo no encontrado en la solicitud" }
                    }
                });
            }

            // Validar extensión del archivo
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".json")
            {
                return BadRequest(new JugadorBatchResultDto
                {
                    Exito = false,
                    Mensaje = "El archivo debe ser de tipo JSON (.json)",
                    Errores = new List<JugadorBatchErrorDto>
                    {
                        new JugadorBatchErrorDto { Error = $"Extensión de archivo inválida: {extension}" }
                    }
                });
            }

            try
            {
                // Procesar el archivo usando el servicio
                var result = await _batchService.ProcessBatchFileAsync(file);

                // Determinar código de estado HTTP según resultado
                if (result.Exito)
                {
                    _logger.LogInformation($"Batch procesado exitosamente: {result.TotalExitosos} jugadores creados");
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning($"Batch con errores: {result.TotalErrores} errores encontrados");
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al procesar batch de jugadores");
                return StatusCode(500, new JugadorBatchResultDto
                {
                    Exito = false,
                    Mensaje = "Error interno del servidor al procesar el archivo",
                    Errores = new List<JugadorBatchErrorDto>
                    {
                        new JugadorBatchErrorDto { Error = $"Error del sistema: {ex.Message}" }
                    }
                });
            }
        }

        // ==========================================
        // FIN NUEVO ENDPOINT BATCH
        // ==========================================

        // PUT: api/Jugador/5
        [HttpPut("{id}")]
        public async Task<ActionResult<JugadorResponseDto>> ActualizarJugador(int id, ActualizarJugadorDto dto)
        {
            var jugador = await _context.Jugadores.FindAsync(id);

            if (jugador == null)
            {
                return NotFound(new { mensaje = "Jugador no encontrado" });
            }

            // Validar equipo NFL si se proporciona
            if (dto.EquipoNFLId.HasValue && dto.EquipoNFLId.Value > 0)
            {
                var equipoExiste = await _context.EquiposNFL.AnyAsync(e => e.Id == dto.EquipoNFLId.Value);
                if (!equipoExiste)
                {
                    return BadRequest(new { mensaje = "El equipo NFL especificado no existe" });
                }
            }

            // Validar nombre duplicado si se está cambiando el nombre o equipo
            if (!string.IsNullOrWhiteSpace(dto.Nombre) || dto.EquipoNFLId.HasValue)
            {
                var nombreParaValidar = !string.IsNullOrWhiteSpace(dto.Nombre) ? dto.Nombre : jugador.Nombre;
                var equipoIdParaValidar = dto.EquipoNFLId ?? jugador.EquipoNFLId;
                var nombreDuplicado = await _context.Jugadores
                    .AnyAsync(j => j.Id != id && 
                                  j.Nombre.ToLower() == nombreParaValidar.ToLower() && 
                                  j.EquipoNFLId == equipoIdParaValidar);

                if (nombreDuplicado)
                {
                    return Conflict(new { mensaje = "Ya existe otro jugador con ese nombre en el equipo NFL especificado" });
                }
            }

            // Actualizar propiedades
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                jugador.Nombre = dto.Nombre.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Posicion))
                jugador.Posicion = dto.Posicion.Trim();

            if (dto.EquipoNFLId.HasValue && dto.EquipoNFLId.Value > 0)
                jugador.EquipoNFLId = dto.EquipoNFLId.Value;

            if (dto.ImagenUrl != null)
                jugador.ImagenUrl = dto.ImagenUrl.Trim();

            if (dto.ThumbnailUrl != null)
                jugador.ThumbnailUrl = dto.ThumbnailUrl.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Estado))
                jugador.Estado = dto.Estado.Trim();

            jugador.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recargar el equipo NFL si cambió
            if (dto.EquipoNFLId.HasValue)
            {
                await _context.Entry(jugador)
                    .Reference(j => j.EquipoNFL)
                    .LoadAsync();
            }

            var response = new JugadorResponseDto
            {
                Id = jugador.Id,
                Nombre = jugador.Nombre,
                Posicion = jugador.Posicion,
                EquipoNFLId = jugador.EquipoNFLId,
                NombreEquipoNFL = jugador.EquipoNFL.Nombre,
                CiudadEquipoNFL = jugador.EquipoNFL.Ciudad,
                ImagenUrl = jugador.ImagenUrl,
                ThumbnailUrl = jugador.ThumbnailUrl,
                Estado = jugador.Estado,
                FechaCreacion = jugador.FechaCreacion,
                FechaActualizacion = jugador.FechaActualizacion
            };

            return Ok(response);
        }

        // DELETE: api/Jugador/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarJugador(int id)
        {
            var jugador = await _context.Jugadores.FindAsync(id);

            if (jugador == null)
            {
                return NotFound(new { mensaje = "Jugador no encontrado" });
            }

            // En lugar de eliminar físicamente, desactivar el jugador
            jugador.Estado = "Inactivo";
            jugador.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Jugador desactivado correctamente" });
        }

        // DELETE: api/Jugador/5/permanente
        [HttpDelete("{id}/permanente")]
        public async Task<IActionResult> EliminarJugadorPermanente(int id)
        {
            var jugador = await _context.Jugadores.FindAsync(id);

            if (jugador == null)
            {
                return NotFound(new { mensaje = "Jugador no encontrado" });
            }

            _context.Jugadores.Remove(jugador);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Jugador eliminado permanentemente" });
        }

        // GET: api/Jugador/Posicion/{posicion}
        [HttpGet("Posicion/{posicion}")]
        public async Task<ActionResult<IEnumerable<JugadorListDto>>> GetJugadoresPorPosicion(string posicion)
        {
            var jugadores = await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .Where(j => j.Posicion.ToLower() == posicion.ToLower())
                .Select(j => new JugadorListDto
                {
                    Id = j.Id,
                    Nombre = j.Nombre,
                    Posicion = j.Posicion,
                    NombreEquipoNFL = j.EquipoNFL.Nombre,
                    ThumbnailUrl = j.ThumbnailUrl,
                    Estado = j.Estado
                })
                .ToListAsync();

            return Ok(jugadores);
        }
    }
}