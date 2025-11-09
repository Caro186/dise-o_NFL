using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para desbloquear una cuenta de usuario
    /// </summary>
    public class DesbloquearCuentaDto
    {
        /// <summary>
        /// Email del usuario cuya cuenta se va a desbloquear
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;
    }
}
