using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipoNFLController : ControllerBase
    {
        private readonly IEquipoNFLService _service;

        public EquipoNFLController(IEquipoNFLService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EquipoNFLCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("{id}/imagen")]
        public async Task<IActionResult> UploadImage(int id, IFormFile imagen)
        {
            var result = await _service.UploadImagenAsync(id, imagen);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }
    }
}
