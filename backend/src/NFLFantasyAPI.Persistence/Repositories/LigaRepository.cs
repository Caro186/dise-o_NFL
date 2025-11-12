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

        public async Task<List<Liga>> GetAllAsync()
            => await _context.Ligas
                .Include(l => l.Comisionado)
                .Include(l => l.Temporada)
                .ToListAsync();

        public async Task<Liga?> GetByIdAsync(int id)
            => await _context.Ligas
                .Include(l => l.Comisionado)
                .Include(l => l.Temporada)
                .FirstOrDefaultAsync(l => l.IdLiga == id);

        public async Task<List<Liga>> GetByComisionadoAsync(int usuarioId)
            => await _context.Ligas
                .Include(l => l.Comisionado)
                .Include(l => l.Temporada)
                .Where(l => l.ComisionadoId == usuarioId)
                .ToListAsync();

        public async Task<List<Liga>> GetByUsuarioAsync(int usuarioId)
        {
            var ligasIds = await _context.EquiposFantasy
                .Where(e => e.UsuarioId == usuarioId && e.LigaId.HasValue)
                .Select(e => e.LigaId!.Value)
                .Distinct()
                .ToListAsync();

            return await _context.Ligas
                .Include(l => l.Comisionado)
                .Include(l => l.Temporada)
                .Where(l => ligasIds.Contains(l.IdLiga))
                .ToListAsync();
        }

        public async Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null)
            => await _context.Ligas.AnyAsync(l => l.NombreLiga == nombre && (excludeId == null || l.IdLiga != excludeId));

        public async Task AddAsync(Liga liga)
            => await _context.Ligas.AddAsync(liga);

        public async Task UpdateAsync(Liga liga)
            => _context.Ligas.Update(liga);

        public async Task DeleteAsync(Liga liga)
            => _context.Ligas.Remove(liga);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
