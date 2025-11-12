using NFLFantasyAPI.Persistence.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface ILigaRepository
    {
        Task<List<Liga>> GetAllAsync();
        Task<Liga?> GetByIdAsync(int id);
        Task<List<Liga>> GetByComisionadoAsync(int usuarioId);
        Task<List<Liga>> GetByUsuarioAsync(int usuarioId);
        Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null);
        Task AddAsync(Liga liga);
        Task UpdateAsync(Liga liga);
        Task DeleteAsync(Liga liga);
        Task SaveChangesAsync();
    }
}
