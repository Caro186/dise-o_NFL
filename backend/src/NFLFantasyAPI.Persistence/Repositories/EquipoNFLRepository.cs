using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class EquipoNFLRepository : IEquipoNFLRepository
    {
        private readonly ApplicationDbContext _context;

        public EquipoNFLRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EquipoNFL>> GetAllAsync()
            => await _context.EquiposNFL.ToListAsync();

        public async Task<EquipoNFL?> GetByIdAsync(int id)
            => await _context.EquiposNFL.FindAsync(id);

        public async Task<bool> ExistsByNameAsync(string nombre)
            => await _context.EquiposNFL.AnyAsync(e => e.Nombre == nombre);

        public async Task AddAsync(EquipoNFL equipo)
            => await _context.EquiposNFL.AddAsync(equipo);

        public async Task DeleteAsync(EquipoNFL equipo)
            => _context.EquiposNFL.Remove(equipo);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
