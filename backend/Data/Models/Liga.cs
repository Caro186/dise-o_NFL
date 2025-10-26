using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Models
{
    public class Liga
    {
        [Key]
        public int IdLiga { get; set; }

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        [Required(ErrorMessage = "El nombre de la liga es obligatorio")]
        [MaxLength(150)]
        public string NombreLiga { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Contrasena obligatoria")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "nombre temporada obligatoria")]
        [MaxLength(50)]
        public string Temporada { get; set; } = string.Empty;  // Ej: "2024-2025"

        [Required(ErrorMessage = "Estado de liga obligatorio")]
        [MaxLength(20)]
        public string Estado { get; set; } = "activa";  // 'activa', 'inactiva', 'finalizada'

        [Required(ErrorMessage = "Cupos totales requeridos")]
        public int CuposTotales { get; set; }

        public int CuposOcupados { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Required(ErrorMessage = "usuario creador obligatorio")]
        public int IdCreador { get; set; }  // FK hacia tabla usuarios
    }
}
