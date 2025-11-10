using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Persistence.Models
{
    /// <summary>
    /// Representa un equipo real de la NFL
    /// </summary>
    public class EquipoNFL
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [MaxLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string Ciudad { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Estado { get; set; } = "Activo";
    }
}
