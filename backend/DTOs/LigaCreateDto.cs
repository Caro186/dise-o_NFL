using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.DTOs
{
    /// <summary>
    /// DTO para creación de liga (solicitud POST)
    /// </summary>
    public class LigaCreateDto
    {
        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        [Required(ErrorMessage = "El nombre de la liga es obligatorio")]
        [MaxLength(150)]
        public string NombreLiga { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Contraseña obligatoria")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nombre de temporada obligatorio")]
        [MaxLength(50)]
        public string Temporada { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Estado { get; set; } = "activa";

        [Required(ErrorMessage = "Cupos totales requeridos")]
        public int CuposTotales { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Required(ErrorMessage = "Usuario creador obligatorio")]
        public int IdCreador { get; set; }
    }
}
