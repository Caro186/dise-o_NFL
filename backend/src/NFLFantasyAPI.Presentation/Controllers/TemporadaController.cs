using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemporadaController : ControllerBase
    {
        private readonly ITemporadaService _temporadaService;

        public TemporadaController(ITemporadaService temporadaService)
        {
            _temporadaService = temporadaService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearTemporada([FromBody] CrearTemporadaDto dto)
        {
            var result = await _temporadaService.CrearTemporadaAsync(dto);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTemporadas()
        {
            var result = await _temporadaService.ObtenerTemporadasAsync();
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerTemporada(int id)
        {
            var result = await _temporadaService.ObtenerTemporadaAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPut("{id}/marcar-actual")]
        public async Task<IActionResult> MarcarComoActual(int id)
        {
            var result = await _temporadaService.MarcarComoActualAsync(id);
            return StatusCode(result.StatusCode, result.Data);
        }
    }
}
