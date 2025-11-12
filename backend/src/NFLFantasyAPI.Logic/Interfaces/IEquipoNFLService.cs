using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Logic.Interfaces
{
    public interface IEquipoNFLService
    {
        Task<ServiceResult> GetAllAsync();
        Task<ServiceResult> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(EquipoNFLCreateDto equipoDto);
        Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
