using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.DTOs;

namespace NFLFantasyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemporadaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TemporadaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/temporada
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemporadaDTO>>> GetTemporadas()
        {
            var temporadas = await _context.Temporadas
                .Include(t => t.Semanas)
                .ToListAsync();

            return temporadas.Select(t => new TemporadaDTO
            {
                Nombre = t.Nombre,
                FechaInicio = t.FechaInicio,
                FechaCierre = t.FechaCierre,
                Actual = t.Actual,
                Semanas = t.Semanas.Select(s => new SemanaDTO
                {
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin
                }).ToList()
            }).ToList();
        }

        // GET: api/temporada/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TemporadaDTO>> GetTemporada(int id)
        {
            var temporada = await _context.Temporadas
                .Include(t => t.Semanas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (temporada == null)
                return NotFound();

            var dto = new TemporadaDTO
            {
                Nombre = temporada.Nombre,
                FechaInicio = temporada.FechaInicio,
                FechaCierre = temporada.FechaCierre,
                Actual = temporada.Actual,
                Semanas = temporada.Semanas.Select(s => new SemanaDTO
                {
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin
                }).ToList()
            };

            return dto;
        }

        // POST: api/temporada
        [HttpPost]
        public async Task<ActionResult<TemporadaDTO>> CrearTemporada([FromBody] TemporadaDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validación extra: FechaInicio < FechaCierre
            if (dto.FechaInicio > dto.FechaCierre)
                return BadRequest("La fecha de inicio no puede ser mayor que la fecha de cierre.");

            var temporada = new Temporada
            {
                Nombre = dto.Nombre,
                FechaInicio = dto.FechaInicio,
                FechaCierre = dto.FechaCierre,
                Actual = dto.Actual,
                FechaCreacion = DateTime.UtcNow,
                Semanas = dto.Semanas?.Select(s => new Semana
                {
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin
                }).ToList()
            };

            // Solo una temporada puede ser actual
            if (temporada.Actual)
            {
                var actuales = await _context.Temporadas.Where(t => t.Actual).ToListAsync();
                foreach (var t in actuales) t.Actual = false;
            }

            _context.Temporadas.Add(temporada);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTemporada), new { id = temporada.Id }, dto);
        }

        // PUT: api/temporada/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarTemporada(int id, [FromBody] TemporadaDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var temporada = await _context.Temporadas
                .Include(t => t.Semanas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (temporada == null)
                return NotFound();

            // Actualizar campos
            temporada.Nombre = dto.Nombre;
            temporada.FechaInicio = dto.FechaInicio;
            temporada.FechaCierre = dto.FechaCierre;

            // Actualizar semanas: borrar existentes y agregar nuevas
            temporada.Semanas.Clear();
            if (dto.Semanas != null && dto.Semanas.Count > 0)
            {
                temporada.Semanas = dto.Semanas.Select(s => new Semana
                {
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin
                }).ToList();
            }

            // Actualizar bandera "Actual"
            if (dto.Actual)
            {
                var actuales = await _context.Temporadas.Where(t => t.Actual && t.Id != id).ToListAsync();
                foreach (var t in actuales) t.Actual = false;

                temporada.Actual = true;
            }
            else
            {
                temporada.Actual = false;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/temporada/5/actual
        [HttpPut("{id}/actual")]
        public async Task<IActionResult> MarcarActual(int id)
        {
            var temporada = await _context.Temporadas.FirstOrDefaultAsync(t => t.Id == id);
            if (temporada == null)
                return NotFound();

            // Desmarcar otras
            var actuales = await _context.Temporadas.Where(t => t.Actual && t.Id != id).ToListAsync();
            foreach (var t in actuales) t.Actual = false;

            temporada.Actual = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
