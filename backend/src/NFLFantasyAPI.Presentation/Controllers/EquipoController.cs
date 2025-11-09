using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.CrossCutting;

namespace NFLFantasyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipoController : ControllerBase
    {
        private readonly IEquipoService _equipoService;
        private readonly IWebHostEnvironment _env;

        public EquipoController(IEquipoService equipoService, IWebHostEnvironment env)
        {
            _equipoService = equipoService;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EquipoCreateDto dto)
        {
            var result = await _equipoService.CreateEquipoAsync(dto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _equipoService.GetEquipoAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuario(int usuarioId)
        {
            var result = await _equipoService.GetEquiposByUsuarioAsync(usuarioId);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _equipoService.GetAllEquiposAsync();
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("{id}/imagen")]
        public async Task<IActionResult> Upload(int id, IFormFile imagen)
        {
            var result = await _equipoService.UploadImagenAsync(id, imagen, _env.WebRootPath);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _equipoService.DeleteEquipoAsync(id, _env.WebRootPath);
            return StatusCode(result.StatusCode, result.Data);
        }
    }
}
