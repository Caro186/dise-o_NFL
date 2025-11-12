using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Logic.Services;
using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LigaController : ControllerBase
    {
        private readonly ILigaService _ligaService;

        public LigaController(ILigaService ligaService)
        {
            _ligaService = ligaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _ligaService.GetAllAsync();
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _ligaService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("comisionado/{usuarioId}")]
        public async Task<IActionResult> GetByComisionado(int usuarioId)
        {
            var result = await _ligaService.GetByComisionadoAsync(usuarioId);
            return StatusCode(result.StatusCode, result.Data);
        }


        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(List<LigaResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetLigasPorUsuario(int usuarioId){
            var result = await _ligaService.GetByUsuarioAsync(usuarioId);
            return StatusCode(result.StatusCode,result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LigaCreateDto dto)
        {
            var result = await _ligaService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result.Data);
        }


        [HttpPost("unirse")]
        public async Task<ActionResult> UnirseLiga([FromBody] UnirseLigaDto dto){
            var result = await _ligaService.UnirseLigaAsync(dto);
            return StatusCode(result.StatusCode, result.Data);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LigaCreateDto dto)
        {
            var result = await _ligaService.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _ligaService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("{id}/imagen")]
        public async Task<IActionResult> UploadImagen(int id, IFormFile imagen)
        {
            var result = await _ligaService.UploadImagenAsync(id, imagen);
            return StatusCode(result.StatusCode, result.Data);
        }
    }
}
