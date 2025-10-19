namespace NFLFantasyAPI.DTOs
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
    }
}