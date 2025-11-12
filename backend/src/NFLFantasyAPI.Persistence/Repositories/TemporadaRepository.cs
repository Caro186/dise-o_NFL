using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class TemporadaRepository : ITemporadaRepository
    {
        private readonly ApplicationDbContext _context;

        public TemporadaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByNameAsync(string nombre)
            => await _context.Temporadas.AnyAsync(t => t.Nombre == nombre);

        public async Task<bool> HasOverlapAsync(DateTime inicio, DateTime cierre)
            => await _context.Temporadas.AnyAsync(t =>
                t.FechaInicio <= cierre && t.FechaCierre >= inicio);

        public async Task<List<Temporada>> GetAllAsync()
            => await _context.Temporadas.Include(t => t.Semanas).ToListAsync();

        public async Task<Temporada?> GetByIdAsync(int id)
            => await _context.Temporadas.Include(t => t.Semanas).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Temporada?> GetActualesAsync()
            => await _context.Temporadas
                             .Where(t => t.Actual)
                             .FirstOrDefaultAsync();

        public async Task AddAsync(Temporada temporada)
            => await _context.Temporadas.AddAsync(temporada);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
