namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para respuesta de información de usuario (sin contraseña)
    /// </summary>
    public class UsuarioResponseDto
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de registro
        /// </summary>
        public DateTime FechaRegistro { get; set; }
    }
}
