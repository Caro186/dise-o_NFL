using Microsoft.AspNetCore.Http;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using System.Threading.Tasks;

namespace NFLFantasyAPI.Logic.Services
{
    public interface ILigaService
    {
        Task<ServiceResult> GetAllAsync();
        Task<ServiceResult> GetByIdAsync(int id);
        Task<ServiceResult> GetByComisionadoAsync(int usuarioId);
        Task<ServiceResult> GetByUsuarioAsync(int usuarioId);
        Task<ServiceResult> CreateAsync(LigaCreateDto dto);
        Task<ServiceResult> UpdateAsync(int id, LigaCreateDto dto);
        Task<ServiceResult> DeleteAsync(int id);
        Task<ServiceResult> UnirseLigaAsync(UnirseLigaDto dto);
        Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen);
    }
}
