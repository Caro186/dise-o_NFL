using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.DTOs;

namespace NFLFantasyAPI.Controllers
{
    /// <summary>
    /// Controlador para gestión de temporadas
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TemporadaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TemporadaController> _logger;

        public TemporadaController(ApplicationDbContext context, ILogger<TemporadaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva temporada con sus semanas
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CrearTemporada([FromBody] CrearTemporadaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos inválidos",
                        Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                // Validar que el nombre de temporada sea único
                if (await _context.Temporadas.AnyAsync(t => t.Nombre == dto.Nombre))
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Ya existe una temporada con ese nombre"
                    });
                }

                // Validar que fechas sean coherentes
                if (dto.FechaInicio >= dto.FechaCierre)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "La fecha de inicio debe ser anterior a la fecha de cierre"
                    });
                }

                // Validar que no haya traslape con otras temporadas
                var hayTraslape = await _context.Temporadas
                    .AnyAsync(t => t.FechaInicio <= dto.FechaCierre && t.FechaCierre >= dto.FechaInicio);

                if (hayTraslape)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Las fechas se traslapan con otra temporada existente"
                    });
                }

                // Si se marca como actual, desmarcar las demás
                if (dto.Actual)
                {
                    var temporadasActuales = await _context.Temporadas
                        .Where(t => t.Actual)
                        .ToListAsync();

                    foreach (var temp in temporadasActuales)
                    {
                        temp.Actual = false;
                    }
                }

                // Crear la temporada
                var temporada = new Temporada
                {
                    Nombre = dto.Nombre,
                    FechaInicio = dto.FechaInicio,
                    FechaCierre = dto.FechaCierre,
                    FechaCreacion = DateTime.UtcNow,
                    Actual = dto.Actual
                };

                _context.Temporadas.Add(temporada);
                await _context.SaveChangesAsync();

                // Crear las semanas si las hay
                if (dto.Semanas != null && dto.Semanas.Any())
                {
                    foreach (var semanaDto in dto.Semanas)
                    {
                        // Validar que las fechas de la semana estén dentro del rango de la temporada
                        if (semanaDto.FechaInicio < temporada.FechaInicio || 
                            semanaDto.FechaFin > temporada.FechaCierre)
                        {
                            return BadRequest(new ErrorResponseDto
                            {
                                Mensaje = "Las fechas de las semanas deben estar dentro del rango de la temporada"
                            });
                        }

                        var semana = new Semana
                        {
                            FechaInicio = semanaDto.FechaInicio,
                            FechaFin = semanaDto.FechaFin,
                            TemporadaId = temporada.Id
                        };

                        _context.Semanas.Add(semana);
                    }

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Temporada creada: {Nombre}", temporada.Nombre);

                var response = new TemporadaResponseDto
                {
                    Id = temporada.Id,
                    Nombre = temporada.Nombre,
                    FechaInicio = temporada.FechaInicio,
                    FechaCierre = temporada.FechaCierre,
                    FechaCreacion = temporada.FechaCreacion,
                    Actual = temporada.Actual
                };

                    return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear temporada");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error al crear la temporada"
                });
            }
        }

        /// <summary>
        /// Obtiene todas las temporadas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ObtenerTemporadas()
        {
            try
            {
                var temporadas = await _context.Temporadas
                    .Include(t => t.Semanas)
                    .Select(t => new TemporadaResponseDto
                    {
                        Id = t.Id,
                        Nombre = t.Nombre,
                        FechaInicio = t.FechaInicio,
                        FechaCreacion = t.FechaCreacion,
                        FechaCierre = t.FechaCierre,
                        Actual = t.Actual
                    })
                    .ToListAsync();

                return Ok(temporadas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener temporadas");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error al obtener temporadas"
                });
            }
        }

        /// <summary>
        /// Obtiene una temporada por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> ObtenerTemporada(int id)
        {
            try
            {
                var temporada = await _context.Temporadas
                    .Include(t => t.Semanas)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (temporada == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Temporada no encontrada"
                    });
                }

                var response = new TemporadaResponseDto
                {
                    Id = temporada.Id,
                    Nombre = temporada.Nombre,
                    FechaInicio = temporada.FechaInicio,
                    FechaCierre = temporada.FechaCierre,
                    FechaCreacion = temporada.FechaCreacion,
                    Actual = temporada.Actual
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener temporada");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error al obtener la temporada"
                });
            }
        }

        /// <summary>
        /// Marca una temporada como actual
        /// </summary>
        [HttpPut("{id}/marcar-actual")]
        public async Task<ActionResult> MarcarComoActual(int id)
        {
            try
            {
                var temporada = await _context.Temporadas.FindAsync(id);

                if (temporada == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Mensaje = "Temporada no encontrada"
                    });
                }

                // Desmarcar todas las demás
                var temporadasActuales = await _context.Temporadas
                    .Where(t => t.Actual && t.Id != id)
                    .ToListAsync();

                foreach (var temp in temporadasActuales)
                {
                    temp.Actual = false;
                }

                temporada.Actual = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Temporada {Id} marcada como actual", id);

                return Ok(new { mensaje = "Temporada marcada como actual" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar temporada como actual");
                return StatusCode(500, new ErrorResponseDto
                {
                    Mensaje = "Error al marcar la temporada como actual"
                });
            }
        }
    }
}