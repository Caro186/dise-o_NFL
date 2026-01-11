using NFLFantasyAPI.Logic.DTOs;

namespace NFLFantasyAPI.Logic.Interfaces
{
    public interface INoticiaJugadorService
    {
        Task<NoticiaJugadorResponseDto> CrearNoticiaAsync(CrearNoticiaJugadorDto dto, int autorId);
        Task<List<NoticiaJugadorResponseDto>> ObtenerNoticiasJugadorAsync(int jugadorId);
        Task<JugadorConNoticiasDto?> ObtenerJugadorConNoticiasAsync(int jugadorId);
        Task<List<NoticiaJugadorResponseDto>> ObtenerTodasNoticiasAsync();
        Task<NoticiaJugadorResponseDto?> ObtenerNoticiaPorIdAsync(int id);
    }
}
