using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para crear equipo NFL
    /// </summary>
    public class EquipoNFLCreateDto
    {
        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [MaxLength(100)]
        public string Ciudad { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para respuesta de equipo NFL
    /// </summary>
    public class EquipoNFLResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
