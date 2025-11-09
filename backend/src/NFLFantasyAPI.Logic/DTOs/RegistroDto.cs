using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para solicitud de registro de usuario
    /// </summary>
    public class RegistroDto
    {
        /// <summary>
        /// Email del usuario
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(50, ErrorMessage = "El email no puede exceder 50 caracteres")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z0-9]{8,12}$",
            ErrorMessage = "La contraseña debe tener entre 8 y 12 caracteres alfanuméricos, con al menos una mayúscula y una minúscula")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [MaxLength(50, ErrorMessage = "El nombre completo no puede exceder 50 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
