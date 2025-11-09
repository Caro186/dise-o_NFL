using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Logic.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult> RegisterAsync(RegistroDto registroDto);
        Task<ServiceResult> LoginAsync(LoginDto loginDto);
        Task<ServiceResult> DesbloquearCuentaAsync(string email);
        Task<ServiceResult> GetUsuariosAsync();
        Task<ServiceResult> GetUsuarioAsync(int id);
        Task<ServiceResult> DeleteUsuarioAsync(int id);
    }
}
