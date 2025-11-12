using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.Services;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JugadorController : ControllerBase
    {
        private readonly IJugadorService _service;
        private readonly ILogger<JugadorController> _logger;

        public JugadorController(
            IJugadorService service,
            ILogger<JugadorController> logger
        )
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            StatusCodeFromResult(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) =>
            StatusCodeFromResult(await _service.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearJugadorDto dto) =>
            StatusCodeFromResult(await _service.CreateAsync(dto));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarJugadorDto dto) =>
            StatusCodeFromResult(await _service.UpdateAsync(id, dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) =>
            StatusCodeFromResult(await _service.DeleteAsync(id, false));

        [HttpDelete("{id}/permanente")]
        public async Task<IActionResult> DeletePermanent(int id) =>
            StatusCodeFromResult(await _service.DeleteAsync(id, true));

        [HttpGet("Equipo/{equipoId}")]
        public async Task<IActionResult> GetByEquipo(int equipoId) =>
            StatusCodeFromResult(await _service.GetByEquipoAsync(equipoId));

        [HttpGet("Posicion/{posicion}")]
        public async Task<IActionResult> GetByPosicion(string posicion) =>
            StatusCodeFromResult(await _service.GetByPosicionAsync(posicion));

        private IActionResult StatusCodeFromResult(ServiceResult result)
        {
            return StatusCode(result.StatusCode, result.Data);
        }


        [HttpPost("{id}/imagen")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirImagen(int id, IFormFile imagen)
        {
            if (imagen == null)
                return BadRequest(new { mensaje = "No se proporcionó ningún archivo" });

            var result = await _service.SubirImagenAsync(id, imagen);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("{id}/thumbnail")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirThumbnail(int id, IFormFile thumbnail)
        {
            if (thumbnail == null)
                return BadRequest(new { mensaje = "No se proporcionó ningún archivo" });

            var result = await _service.SubirThumbnailAsync(id, thumbnail);
            return StatusCode(result.StatusCode, result.Data);
        }

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
                var result = await _service.ProcessBatchFileAsync(file);

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


    }
}
