using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;

namespace NFLFantasyAPI.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JugadorController : ControllerBase
    {
        private readonly IJugadorService _service;

        public JugadorController(IJugadorService service)
        {
            _service = service;
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
    }
}
