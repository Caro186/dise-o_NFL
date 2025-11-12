using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface IEquipoFantasyRepository
    {
        Task<List<EquipoFantasy>> GetAllAsync();
        Task<EquipoFantasy?> GetByIdAsync(int id);
        Task<List<EquipoFantasy>> GetByUsuarioIdAsync(int usuarioId);
        Task<bool> NombreExisteAsync(string nombre, int usuarioId);
        Task AddAsync(EquipoFantasy equipo);
        Task DeleteAsync(EquipoFantasy equipo);
        Task<bool> UsuarioExisteAsync(int usuarioId);
        Task SaveChangesAsync();
        Task UpdateAsync(EquipoFantasy equipo);
    }
}
