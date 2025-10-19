using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.DTOs
{
    /// <summary>
    /// DTO para solicitud de creación de equipo
    /// </summary>
    public class EquipoCreateDto
    {
        /// <summary>
        /// Nombre del equipo
        /// </summary>
        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre del equipo no puede exceder 100 caracteres")]
        [MinLength(1, ErrorMessage = "El nombre del equipo debe tener al menos 1 caracter")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// ID del usuario propietario
        /// </summary>
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Liga del equipo (opcional)
        /// </summary>
        [MaxLength(50, ErrorMessage = "La liga no puede exceder 50 caracteres")]
        public string? Liga { get; set; }
    }
}