using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;

namespace NFLFantasyAPI.Persistence.Repositories
{
    public class NoticiaJugadorRepository : INoticiaJugadorRepository
    {
        private readonly ApplicationDbContext _context;

        public NoticiaJugadorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NoticiaJugador?> ObtenerPorIdAsync(int id)
        {
            return await _context.NoticiasJugador
                .Include(n => n.Jugador)
                    .ThenInclude(j => j.EquipoNFL)
                .Include(n => n.Autor)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<List<NoticiaJugador>> ObtenerPorJugadorAsync(int jugadorId)
        {
            return await _context.NoticiasJugador
                .Include(n => n.Autor)
                .Where(n => n.JugadorId == jugadorId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<NoticiaJugador> CrearAsync(NoticiaJugador noticia)
        {
            _context.NoticiasJugador.Add(noticia);
            await _context.SaveChangesAsync();
            return noticia;
        }

        public async Task<List<NoticiaJugador>> ObtenerTodasAsync()
        {
            return await _context.NoticiasJugador
                .Include(n => n.Jugador)
                    .ThenInclude(j => j.EquipoNFL)
                .Include(n => n.Autor)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<bool> ExisteJugadorAsync(int jugadorId)
        {
            return await _context.Jugadores.AnyAsync(j => j.Id == jugadorId && j.Estado == "Activo");
        }

        public async Task<Jugador?> ObtenerJugadorConNoticiasAsync(int jugadorId)
        {
            return await _context.Jugadores
                .Include(j => j.EquipoNFL)
                .Include(j => j.Noticias.OrderByDescending(n => n.FechaCreacion))
                    .ThenInclude(n => n.Autor)
                .FirstOrDefaultAsync(j => j.Id == jugadorId);
        }

        public async Task ActualizarDesignacionJugadorAsync(int jugadorId, string? designacion)
        {
            var jugador = await _context.Jugadores.FindAsync(jugadorId);
            if (jugador != null)
            {
                jugador.DesignacionLesion = designacion;
                jugador.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
