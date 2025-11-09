namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para respuesta de login exitoso
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// Estado de la operación
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Información del usuario autenticado
        /// </summary>
        public UsuarioResponseDto? Usuario { get; set; }

        /// <summary>
        /// Token JWT para autenticación
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Fecha de expiración del token
        /// </summary>
        public DateTime? TokenExpiracion { get; set; }
    }
}
