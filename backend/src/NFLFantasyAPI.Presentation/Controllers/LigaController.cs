using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Controllers
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

        [HttpPost]
        public async Task<IActionResult> CrearLiga([FromBody] LigaCreateDto dto)
        {
            var result = await _ligaService.CrearLigaAsync(dto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("unirse")]
        public async Task<IActionResult> UnirseALiga([FromBody] UnirseALigaDto dto)
        {
            var result = await _ligaService.UnirseALigaAsync(dto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerLiga(int id)
        {
            var result = await _ligaService.ObtenerLigaAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var result = await _ligaService.ObtenerTodasAsync();
            return StatusCode(result.StatusCode, result.Data);
        }
    }
}
