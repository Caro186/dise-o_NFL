using Microsoft.AspNetCore.Http;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using System.Threading.Tasks;

namespace NFLFantasyAPI.Logic.Interfaces
{
    public interface IJugadorService
    {
        Task<ServiceResult> GetAllAsync();

        Task<ServiceResult> GetByIdAsync(int id);

        Task<ServiceResult> CreateAsync(CrearJugadorDto dto);

        Task<ServiceResult> UpdateAsync(int id, ActualizarJugadorDto dto);

        Task<ServiceResult> DeleteAsync(int id, bool permanente);

        Task<ServiceResult> GetByEquipoAsync(int equipoId);

        Task<ServiceResult> GetByPosicionAsync(string posicion);

        Task<JugadorBatchResultDto> ProcessBatchFileAsync(IFormFile file);

        Task<ServiceResult> SubirImagenAsync(int id, IFormFile imagen);
        Task<ServiceResult> SubirThumbnailAsync(int id, IFormFile thumbnail);
    }
}

