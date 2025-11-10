using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    // DTO para crear un jugador
    public class CrearJugadorDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La posición es requerida")]
        [MaxLength(50, ErrorMessage = "La posición no puede exceder 50 caracteres")]
        public string Posicion { get; set; }

        [Required(ErrorMessage = "El equipo NFL es requerido")]
        public int EquipoNFLId { get; set; }

        [MaxLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
        public string? ImagenUrl { get; set; }

        [MaxLength(500, ErrorMessage = "La URL del thumbnail no puede exceder 500 caracteres")]
        public string? ThumbnailUrl { get; set; }
    }

    // DTO para actualizar un jugador
    public class ActualizarJugadorDto
    {
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string? Nombre { get; set; }

        [MaxLength(50, ErrorMessage = "La posición no puede exceder 50 caracteres")]
        public string? Posicion { get; set; }

        public int? EquipoNFLId { get; set; }

        [MaxLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
        public string? ImagenUrl { get; set; }

        [MaxLength(500, ErrorMessage = "La URL del thumbnail no puede exceder 500 caracteres")]
        public string? ThumbnailUrl { get; set; }

        [MaxLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string? Estado { get; set; }
    }

    // DTO para respuesta de jugador
    public class JugadorResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Posicion { get; set; }
        public int EquipoNFLId { get; set; }
        public string NombreEquipoNFL { get; set; }
        public string CiudadEquipoNFL { get; set; }
        public string? ImagenUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    // DTO para lista de jugadores
    public class JugadorListDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Posicion { get; set; }
        public string NombreEquipoNFL { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Estado { get; set; }
    }
}
