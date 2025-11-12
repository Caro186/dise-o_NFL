using Microsoft.AspNetCore.Http;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Logic.Interfaces
{
    public interface IEquipoFantasyService
    {
        Task<ServiceResult> GetAllEquiposFantasyAsync();
        Task<ServiceResult> GetEquipoFantasyByIdAsync(int id);
        Task<ServiceResult> GetEquiposByUsuarioAsync(int usuarioId);
        Task<ServiceResult> CreateEquipoFantasyAsync(EquipoFantasyCreateDto dto);
        Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen, string uploadsRoot, string baseUrl);
        Task<ServiceResult> DeleteEquipoFantasyAsync(int id);
    }
}
