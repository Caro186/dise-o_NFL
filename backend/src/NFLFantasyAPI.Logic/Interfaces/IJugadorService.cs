using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;

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
    }
}
