using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.CrossCutting;

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



