namespace NFLFantasyAPI.DTOs
{
    /// <summary>
    /// DTO para respuesta de información de equipo
    /// </summary>
    public class EquipoResponseDto
    {
        /// <summary>
        /// ID del equipo
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del equipo
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// URL de la imagen del equipo
        /// </summary>
        public string? ImagenUrl { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// ID del propietario
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Nombre del propietario
        /// </summary>
        public string? NombrePropietario { get; set; }

        /// <summary>
        /// Estado del equipo
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Liga del equipo
        /// </summary>
        public string? Liga { get; set; }
    }
}