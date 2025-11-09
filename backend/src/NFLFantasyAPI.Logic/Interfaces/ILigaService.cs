using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.CrossCutting;

namespace NFLFantasyAPI.Logic.Interfaces
{
    public interface ILigaService
    {
        Task<ServiceResult> CrearLigaAsync(LigaCreateDto dto);
        Task<ServiceResult> UnirseALigaAsync(UnirseALigaDto dto);
        Task<ServiceResult> ObtenerLigaAsync(int id);
        Task<ServiceResult> ObtenerTodasAsync();
    }
}
