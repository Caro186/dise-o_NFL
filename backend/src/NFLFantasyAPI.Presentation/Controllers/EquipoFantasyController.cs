using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.CrossCutting.Configuration;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipoFantasyController : ControllerBase
    {
        private readonly IEquipoFantasyService _service;
        private readonly IWebHostEnvironment _env;
        private readonly FileServerSettings _fileSettings;

        public EquipoFantasyController(
            IEquipoFantasyService service,
            IWebHostEnvironment env,
            IOptions<FileServerSettings> fileSettings)
        {
            _service = service;
            _env = env;
            _fileSettings = fileSettings.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllEquiposFantasyAsync();
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetEquipoFantasyByIdAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuario(int usuarioId)
        {
            var result = await _service.GetEquiposByUsuarioAsync(usuarioId);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EquipoFantasyCreateDto dto)
        {
            var result = await _service.CreateEquipoFantasyAsync(dto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("{id}/imagen")]
        public async Task<IActionResult> UploadImagen(int id, IFormFile imagen)
        {
            var result = await _service.UploadImagenAsync(
                id, imagen, _env.WebRootPath, _fileSettings.BaseUrl);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteEquipoFantasyAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }
    }
}
