using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para crear equipo Fantasy
    /// </summary>
    public class EquipoFantasyCreateDto
    {
        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int UsuarioId { get; set; }

        public int? LigaId { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de equipo Fantasy
    /// </summary>
    public class EquipoFantasyResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string? NombrePropietario { get; set; }
        public int? LigaId { get; set; }
        public string? NombreLiga { get; set; }
        public string? ImagenUrl { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
