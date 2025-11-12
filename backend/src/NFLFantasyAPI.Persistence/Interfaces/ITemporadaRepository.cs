using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface ITemporadaRepository
    {
        Task<bool> ExistsByNameAsync(string nombre);
        Task<bool> HasOverlapAsync(DateTime inicio, DateTime cierre);
        Task<List<Temporada>> GetAllAsync();
        Task<Temporada?> GetByIdAsync(int id);
        Task<Temporada?> GetActualesAsync();
        Task AddAsync(Temporada temporada);
        Task SaveChangesAsync();
    }
}
