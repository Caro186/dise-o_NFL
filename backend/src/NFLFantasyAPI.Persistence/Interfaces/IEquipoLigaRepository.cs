using System.Collections.Generic;
using System.Threading.Tasks;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface IEquipoLigaRepository
    {
        Task<IEnumerable<EquipoLiga>> GetAllAsync();
        Task<EquipoLiga?> GetByIdAsync(int id);
        Task<IEnumerable<EquipoLiga>> GetByLigaIdAsync(int idLiga);
        Task<IEnumerable<EquipoLiga>> GetByEquipoIdAsync(int idEquipo);
        Task<bool> ExistsAsync(int idEquipo, int idLiga);
        Task AddAsync(EquipoLiga entity);
        void Update(EquipoLiga entity);
        void Delete(EquipoLiga entity);
        Task SaveChangesAsync();
    }
}
