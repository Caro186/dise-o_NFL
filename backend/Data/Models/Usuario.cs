using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Models
{
    /// <summary>
    /// Representa un usuario registrado en el sistema Fantasy NFL
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Correo electrónico del usuario (único en el sistema)
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(50, ErrorMessage = "El email no puede exceder 50 caracteres")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario (almacenada con hash)
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MaxLength(255, ErrorMessage = "El hash de contraseña no puede exceder 255 caracteres")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [MaxLength(50, ErrorMessage = "El nombre completo no puede exceder 50 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de registro del usuario en el sistema
        /// </summary>
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}