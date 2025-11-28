using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Interfaces
{
    public interface INoticiaJugadorRepository
    {
        Task<NoticiaJugador?> ObtenerPorIdAsync(int id);
        Task<List<NoticiaJugador>> ObtenerPorJugadorAsync(int jugadorId);
        Task<NoticiaJugador> CrearAsync(NoticiaJugador noticia);
        Task<List<NoticiaJugador>> ObtenerTodasAsync();
        Task<bool> ExisteJugadorAsync(int jugadorId);
        Task<Jugador?> ObtenerJugadorConNoticiasAsync(int jugadorId);
        Task ActualizarDesignacionJugadorAsync(int jugadorId, string? designacion);
    }
}
