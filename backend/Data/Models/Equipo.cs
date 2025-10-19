using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Models
{
    /// <summary>
    /// Representa un equipo de fantasy creado por un usuario
    /// </summary>
    public class Equipo
    {
        /// <summary>
        /// Identificador único del equipo
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del equipo (único por liga)
        /// </summary>
        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre del equipo no puede exceder 100 caracteres")]
        [MinLength(1, ErrorMessage = "El nombre del equipo debe tener al menos 1 caracter")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// URL o ruta de la imagen del equipo
        /// </summary>
        [MaxLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
        public string? ImagenUrl { get; set; }

        /// <summary>
        /// Fecha de creación del equipo
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID del usuario propietario del equipo
        /// </summary>
        [Required(ErrorMessage = "El equipo debe tener un propietario")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Usuario propietario del equipo (navegación)
        /// </summary>
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// Estado del equipo (Activo/Inactivo)
        /// </summary>
        [MaxLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string Estado { get; set; } = "Activo";

        /// <summary>
        /// Liga a la que pertenece el equipo
        /// </summary>
        [MaxLength(50, ErrorMessage = "La liga no puede exceder 50 caracteres")]
        public string? Liga { get; set; }
    }
}