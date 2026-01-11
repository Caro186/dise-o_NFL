using System.Collections.Generic;
using System.Threading.Tasks;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface IEquipoNFLRepository
    {
        Task<List<EquipoNFL>> GetAllAsync();
        Task<EquipoNFL?> GetByIdAsync(int id);
        Task<bool> ExistsByNameAsync(string nombre);
        Task AddAsync(EquipoNFL equipo);
        Task DeleteAsync(EquipoNFL equipo);
        Task SaveChangesAsync();
    }
}
