using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
            => await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<Usuario?> GetByIdAsync(int id)
            => await _context.Usuarios.FindAsync(id);

        public async Task<List<Usuario>> GetAllAsync()
            => await _context.Usuarios.ToListAsync();

        public async Task AddAsync(Usuario usuario)
            => await _context.Usuarios.AddAsync(usuario);

        public Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Usuario usuario)
        {
            _context.Usuarios.Remove(usuario);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
            => await _context.Usuarios.AnyAsync(u => u.Email == email);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
