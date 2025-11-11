using NFLFantasyAPI.Persistence.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface IJugadorRepository
    {
        Task<List<Jugador>> GetAllAsync();
        Task<Jugador?> GetByIdAsync(int id);
        Task<List<Jugador>> GetByEquipoAsync(int equipoId);
        Task<List<Jugador>> GetByPosicionAsync(string posicion);
        Task<bool> ExistsInEquipoAsync(string nombre, int equipoId);
        Task<bool> EquipoExistsAsync(int equipoId);
        Task AddAsync(Jugador jugador);
        Task UpdateAsync(Jugador jugador);
        Task DeleteAsync(Jugador jugador);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task RollbackTransactionAsync();
        Task SaveChangesAsync();
    }
}
