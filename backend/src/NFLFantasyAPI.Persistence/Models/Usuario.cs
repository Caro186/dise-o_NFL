using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Persistence.Models
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

        /// <summary>
        /// Contador de intentos fallidos de inicio de sesión consecutivos
        /// </summary>
        public int IntentosFailidos { get; set; } = 0;

        /// <summary>
        /// Fecha y hora del último intento fallido de inicio de sesión
        /// </summary>
        public DateTime? FechaUltimoIntentoFallido { get; set; }

        /// <summary>
        /// Estado de la cuenta del usuario (Activa o Bloqueada)
        /// </summary>
        [Required]
        [MaxLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string EstadoCuenta { get; set; } = "Activa";

        /// <summary>
        /// Fecha y hora en que la cuenta fue bloqueada
        /// </summary>
        public DateTime? FechaBloqueo { get; set; }

        /// <summary>
        /// Fecha y hora de la última actividad del usuario (para control de inactividad)
        /// </summary>
        public DateTime? UltimaActividad { get; set; }

        
        public string Rol { get; set; } = "Usuario"; // Valores: "Usuario", "Admin"

    }
}
