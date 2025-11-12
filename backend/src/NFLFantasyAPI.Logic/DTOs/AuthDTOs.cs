using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para registro de usuario
    /// </summary>
    public class RegistroDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [MaxLength(50)]
        public string NombreCompleto { get; set; } = string.Empty;

    }

    /// <summary>
    /// DTO para login
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de respuesta de usuario
    /// </summary>
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        
        public string Rol { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de respuesta de login
    /// </summary>
    public class LoginResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? TokenExpiracion { get; set; }
        public UsuarioDto? Usuario { get; set; }
    }
}
