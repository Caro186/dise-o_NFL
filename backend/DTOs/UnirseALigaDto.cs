using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.DTOs
{
    /// <summary>
    /// DTO para unirse a una liga existente
    /// </summary>
    public class UnirseALigaDto
    {
        /// <summary>
        /// ID de la liga a la que se quiere unir
        /// </summary>
        [Required(ErrorMessage = "El ID de la liga es obligatorio")]
        public int IdLiga { get; set; }

        /// <summary>
        /// ID del usuario que se va a unir
        /// </summary>
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Contraseña de la liga
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(12, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 12 caracteres")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Alias del usuario en esta liga (debe ser único dentro de la liga)
        /// </summary>
        [Required(ErrorMessage = "El alias es obligatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El alias debe tener entre 1 y 50 caracteres")]
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del equipo (debe ser único dentro de la liga)
        /// </summary>
        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre del equipo debe tener entre 1 y 100 caracteres")]
        public string NombreEquipo { get; set; } = string.Empty;
    }
}