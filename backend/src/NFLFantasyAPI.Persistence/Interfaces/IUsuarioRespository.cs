using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByIdAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task AddAsync(Usuario usuario);
        void Remove(Usuario usuario);
        Task SaveChangesAsync();
    }
}
