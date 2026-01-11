namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para respuestas de error estandarizadas
    /// </summary>
    public class ErrorResponseDto
    {
        /// <summary>
        /// Mensaje principal del error
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Lista de errores detallados (opcional)
        /// </summary>
        public List<string>? Errores { get; set; }
    }
}
