using System;

namespace NFLFantasyAPI.Persistence.Models
{
    public class NoticiaJugador
    {
        public int Id { get; set; }

        // Relación con el jugador
        public int JugadorId { get; set; }
        public virtual Jugador? Jugador { get; set; }

        // Contenido de la noticia
        public string Texto { get; set; } = string.Empty;

        // Indicador de lesión
        public bool EsLesion { get; set; }

        // Campos específicos para lesiones
        public string? ResumenLesion { get; set; }
        public string? DesignacionLesion { get; set; }

        // Auditoría
        public int AutorId { get; set; }
        public virtual Usuario? Autor { get; set; }

        public DateTime FechaCreacion { get; set; }

        // Estado de la noticia
        public string Estado { get; set; } = "Activa";
    }
}
