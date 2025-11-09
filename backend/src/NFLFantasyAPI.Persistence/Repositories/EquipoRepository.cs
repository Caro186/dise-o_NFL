using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class EquipoRepository : IEquipoRepository
    {
        private readonly ApplicationDbContext _context;

        public EquipoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByNombreAsync(string nombre, int usuarioId)
        {
            return await _context.Equipos.AnyAsync(e => e.Nombre == nombre && e.UsuarioId == usuarioId);
        }

        public async Task AddAsync(Equipo equipo)
        {
            await _context.Equipos.AddAsync(equipo);
        }

        public async Task<Equipo?> GetByIdAsync(int id)
        {
            return await _context.Equipos.Include(e => e.Usuario).FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Equipo>> GetAllAsync()
        {
            return await _context.Equipos.Include(e => e.Usuario).ToListAsync();
        }

        public async Task<List<Equipo>> GetByUsuarioAsync(int usuarioId)
        {
            return await _context.Equipos
                .Include(e => e.Usuario)
                .Where(e => e.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task DeleteAsync(Equipo equipo)
        {
            _context.Equipos.Remove(equipo);
            await _context.SaveChangesAsync();
        }
    }
}
