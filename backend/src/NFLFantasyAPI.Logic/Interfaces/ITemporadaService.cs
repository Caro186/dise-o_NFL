using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.CrossCutting;
using System.Threading.Tasks;

namespace NFLFantasyAPI.Logic.Interfaces
{
    public interface ITemporadaService
    {
        Task<ServiceResult> CrearTemporadaAsync(CrearTemporadaDto dto);
        Task<ServiceResult> ObtenerTemporadasAsync();
        Task<ServiceResult> ObtenerTemporadaAsync(int id);
        Task<ServiceResult> MarcarComoActualAsync(int id);
    }
}
