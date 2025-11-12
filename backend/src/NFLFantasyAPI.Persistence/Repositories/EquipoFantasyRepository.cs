using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class EquipoFantasyRepository : IEquipoFantasyRepository
    {
        private readonly ApplicationDbContext _context;

        public EquipoFantasyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EquipoFantasy>> GetAllAsync()
            => await _context.EquiposFantasy
                .Include(e => e.Usuario)
                .Include(e => e.Liga)
                .ToListAsync();

        public async Task<EquipoFantasy?> GetByIdAsync(int id)
            => await _context.EquiposFantasy
                .Include(e => e.Usuario)
                .Include(e => e.Liga)
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<List<EquipoFantasy>> GetByUsuarioIdAsync(int usuarioId)
            => await _context.EquiposFantasy
                .Include(e => e.Usuario)
                .Include(e => e.Liga)
                .Where(e => e.UsuarioId == usuarioId)
                .ToListAsync();

        public async Task<bool> NombreExisteAsync(string nombre, int usuarioId)
            => await _context.EquiposFantasy.AnyAsync(e => e.Nombre == nombre && e.UsuarioId == usuarioId);

        public async Task<bool> UsuarioExisteAsync(int usuarioId)
            => await _context.Usuarios.AnyAsync(u => u.Id == usuarioId);

        public async Task AddAsync(EquipoFantasy equipo)
            => await _context.EquiposFantasy.AddAsync(equipo);

        public Task DeleteAsync(EquipoFantasy equipo)
        {
            _context.EquiposFantasy.Remove(equipo);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public Task UpdateAsync(EquipoFantasy equipo)
        {
            _context.EquiposFantasy.Update(equipo);
            return Task.CompletedTask;
        }
    }
}
