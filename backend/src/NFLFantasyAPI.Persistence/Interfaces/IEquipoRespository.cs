using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface IEquipoRepository
    {
        Task<bool> ExistsByNombreAsync(string nombre, int usuarioId);
        Task AddAsync(Equipo equipo);
        Task<Equipo?> GetByIdAsync(int id);
        Task<List<Equipo>> GetAllAsync();
        Task<List<Equipo>> GetByUsuarioAsync(int usuarioId);
        Task SaveChangesAsync();
        Task DeleteAsync(Equipo equipo);
    }
}
