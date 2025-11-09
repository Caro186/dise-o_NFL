using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.CrossCutting; 

namespace NFLFantasyAPI.Logic.Interfaces
{
    public interface IEquipoService
    {
        Task<ServiceResult> CreateEquipoAsync(EquipoCreateDto dto);
        Task<ServiceResult> GetEquipoAsync(int id);
        Task<ServiceResult> GetEquiposByUsuarioAsync(int usuarioId);
        Task<ServiceResult> GetAllEquiposAsync();
        //Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen, string webRootPath);
        //Es necesario actualizar el metodo de imagenes
        Task<ServiceResult> UploadImagenAsync(int id, string webRootPath);
        Task<ServiceResult> DeleteEquipoAsync(int id, string webRootPath);
    }
}
