using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class JugadorRepository : IJugadorRepository
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        public JugadorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Jugador>> GetAllAsync()
        {
            return await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .ToListAsync();
        }

        public async Task<Jugador?> GetByIdAsync(int id)
        {
            return await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<List<Jugador>> GetByEquipoAsync(int equipoId)
        {
            return await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .Where(j => j.EquipoNFLId == equipoId)
                .ToListAsync();
        }

        public async Task<List<Jugador>> GetByPosicionAsync(string posicion)
        {
            return await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .Where(j => j.Posicion.ToLower() == posicion.ToLower())
                .ToListAsync();
        }

        public async Task<bool> ExistsInEquipoAsync(string nombre, int equipoId)
        {
            return await _context.Jugadores.AnyAsync(j =>
                j.Nombre.ToLower() == nombre.ToLower() &&
                j.EquipoNFLId == equipoId);
        }

        public async Task AddAsync(Jugador jugador)
        {
            _context.Jugadores.Add(jugador);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Jugador jugador)
        {
            _context.Jugadores.Update(jugador);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Jugador jugador)
        {
            _context.Jugadores.Remove(jugador);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EquipoExistsAsync(int equipoId)
        {
            return await _context.EquiposNFL.AnyAsync(e => e.Id == equipoId);
        }

         public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
