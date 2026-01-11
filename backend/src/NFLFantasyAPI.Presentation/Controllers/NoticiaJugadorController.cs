using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using System.Security.Claims;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoticiaJugadorController : ControllerBase
    {
        private readonly INoticiaJugadorService _noticiaService;
        private readonly ILogger<NoticiaJugadorController> _logger;

        public NoticiaJugadorController(
            INoticiaJugadorService noticiaService,
            ILogger<NoticiaJugadorController> logger)
        {
            _noticiaService = noticiaService;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva noticia para un jugador (solo administradores)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CrearNoticia([FromBody] CrearNoticiaJugadorDto dto)
        {
            try
            {
                // Obtener el ID del usuario autenticado del token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int autorId))
                {
                    _logger.LogWarning("No se pudo obtener el ID del usuario autenticado");
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });
                }

                _logger.LogInformation("Usuario {AutorId} creando noticia para jugador {JugadorId}", autorId, dto.JugadorId);

                var noticia = await _noticiaService.CrearNoticiaAsync(dto, autorId);

                return CreatedAtAction(
                    nameof(ObtenerNoticiaPorId),
                    new { id = noticia.Id },
                    new
                    {
                        mensaje = "Noticia creada exitosamente",
                        noticia = noticia,
                        auditoria = new
                        {
                            autor = noticia.NombreAutor,
                            autorId = noticia.AutorId,
                            fechaCreacion = noticia.FechaCreacion,
                            accion = "Creación de noticia",
                            cambios = dto.EsLesion
                                ? $"Noticia de lesión creada. Designación: {dto.DesignacionLesion}"
                                : "Noticia general creada"
                        }
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear noticia");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de argumentos al crear noticia");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear noticia");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las noticias de un jugador específico
        /// </summary>
        [HttpGet("jugador/{jugadorId}")]
        public async Task<IActionResult> ObtenerNoticiasJugador(int jugadorId)
        {
            try
            {
                var noticias = await _noticiaService.ObtenerNoticiasJugadorAsync(jugadorId);
                return Ok(new { noticias = noticias, total = noticias.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener noticias del jugador {JugadorId}", jugadorId);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un jugador con todas sus noticias
        /// </summary>
        [HttpGet("jugador/{jugadorId}/completo")]
        public async Task<IActionResult> ObtenerJugadorConNoticias(int jugadorId)
        {
            try
            {
                var jugador = await _noticiaService.ObtenerJugadorConNoticiasAsync(jugadorId);

                if (jugador == null)
                {
                    return NotFound(new { mensaje = "Jugador no encontrado" });
                }

                return Ok(jugador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener jugador con noticias {JugadorId}", jugadorId);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las noticias del sistema
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodasNoticias()
        {
            try
            {
                var noticias = await _noticiaService.ObtenerTodasNoticiasAsync();
                return Ok(new { noticias = noticias, total = noticias.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las noticias");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una noticia específica por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerNoticiaPorId(int id)
        {
            try
            {
                var noticia = await _noticiaService.ObtenerNoticiaPorIdAsync(id);

                if (noticia == null)
                {
                    return NotFound(new { mensaje = "Noticia no encontrada" });
                }

                return Ok(noticia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener noticia {NoticiaId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}
