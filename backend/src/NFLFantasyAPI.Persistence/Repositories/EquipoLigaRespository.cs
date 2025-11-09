using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Context;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class EquipoLigaRepository : IEquipoLigaRepository
    {
        private readonly ApplicationDbContext _context;

        public EquipoLigaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EquipoLiga>> GetAllAsync()
        {
            return await _context.EquiposLigas
                .Include(el => el.Equipo)
                .Include(el => el.Liga)
                .ToListAsync();
        }

        public async Task<EquipoLiga?> GetByIdAsync(int id)
        {
            return await _context.EquiposLigas
                .Include(el => el.Equipo)
                .Include(el => el.Liga)
                .FirstOrDefaultAsync(el => el.Id == id);
        }

        public async Task<IEnumerable<EquipoLiga>> GetByLigaIdAsync(int idLiga)
        {
            return await _context.EquiposLigas
                .Where(el => el.IdLiga == idLiga)
                .Include(el => el.Equipo)
                .ToListAsync();
        }

        public async Task<IEnumerable<EquipoLiga>> GetByEquipoIdAsync(int idEquipo)
        {
            return await _context.EquiposLigas
                .Where(el => el.IdEquipo == idEquipo)
                .Include(el => el.Liga)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int idEquipo, int idLiga)
        {
            return await _context.EquiposLigas
                .AnyAsync(el => el.IdEquipo == idEquipo && el.IdLiga == idLiga);
        }

        public async Task AddAsync(EquipoLiga entity)
        {
            await _context.EquiposLigas.AddAsync(entity);
        }

        public void Update(EquipoLiga entity)
        {
            _context.EquiposLigas.Update(entity);
        }

        public void Delete(EquipoLiga entity)
        {
            _context.EquiposLigas.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
