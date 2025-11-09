using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface ILigaRepository
    {
        Task<Liga?> GetByIdAsync(int id);
        Task<bool> ExistsByNameAsync(string nombreLiga, int temporadaId);
        Task AddAsync(Liga liga);
        Task<List<Liga>> GetAllAsync();
        Task SaveChangesAsync();
    }
}
