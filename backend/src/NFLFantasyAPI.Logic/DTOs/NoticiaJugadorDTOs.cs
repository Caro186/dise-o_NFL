using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    // DTO para crear una noticia
    public class CrearNoticiaJugadorDto
    {
        [Required(ErrorMessage = "El ID del jugador es obligatorio")]
        public int JugadorId { get; set; }

        [Required(ErrorMessage = "El texto de la noticia es obligatorio")]
        [StringLength(300, MinimumLength = 10, ErrorMessage = "El texto debe tener entre 10 y 300 caracteres")]
        public string Texto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar si es una noticia de lesión")]
        public bool EsLesion { get; set; }

        [StringLength(30, ErrorMessage = "El resumen no puede superar los 30 caracteres")]
        public string? ResumenLesion { get; set; }

        [RegularExpression("^(O|D|Q|P|FP|IR|PUP|SUS)$", ErrorMessage = "Designación inválida. Valores permitidos: O, D, Q, P, FP, IR, PUP, SUS")]
        public string? DesignacionLesion { get; set; }
    }

    // DTO para respuesta de noticia
    public class NoticiaJugadorResponseDto
    {
        public int Id { get; set; }
        public int JugadorId { get; set; }
        public string NombreJugador { get; set; } = string.Empty;
        public string EquipoNFL { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public bool EsLesion { get; set; }
        public string? ResumenLesion { get; set; }
        public string? DesignacionLesion { get; set; }
        public string? DesignacionDescripcion { get; set; }
        public int AutorId { get; set; }
        public string NombreAutor { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    // DTO para listar noticias del jugador
    public class JugadorConNoticiasDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Posicion { get; set; } = string.Empty;
        public string EquipoNFL { get; set; } = string.Empty;
        public string? DesignacionLesion { get; set; }
        public string? DesignacionDescripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public List<NoticiaJugadorResponseDto> Noticias { get; set; } = new List<NoticiaJugadorResponseDto>();
    }

    // DTO para resultado de auditoría
    public class AuditoriaNoticiaDto
    {
        public int NoticiaId { get; set; }
        public int AutorId { get; set; }
        public string NombreAutor { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string AccionRealizada { get; set; } = string.Empty;
        public string DetallesCambio { get; set; } = string.Empty;
    }
}
