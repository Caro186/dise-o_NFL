using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class LigaRepository : ILigaRepository
    {
        private readonly ApplicationDbContext _context;

        public LigaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Liga?> GetByIdAsync(int id) =>
            await _context.Ligas.FindAsync(id);

        public async Task<bool> ExistsByNameAsync(string nombreLiga, int temporadaId) =>
            await _context.Ligas.AnyAsync(l => l.NombreLiga == nombreLiga && l.IdTemporada == temporadaId);

        public async Task AddAsync(Liga liga) =>
            await _context.Ligas.AddAsync(liga);

        public async Task<List<Liga>> GetAllAsync() =>
            await _context.Ligas.ToListAsync();

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
