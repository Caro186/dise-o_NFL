namespace NFLFantasyAPI.Persistence.Models
{
    /// <summary>
    /// Resultado interno del procesamiento batch antes de convertir a DTO
    /// </summary>
    public class BatchProcessResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Jugador> CreatedPlayers { get; set; } = new();
        public List<BatchValidationError> Errors { get; set; } = new();
        public string ProcessedFilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Error de validación durante el procesamiento batch
    /// </summary>
    public class BatchValidationError
    {
        public int? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty; // "validation", "duplicate", "not_found"
    }

    /// <summary>
    /// Información del archivo procesado
    /// </summary>
    public class ProcessedFileInfo
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string ProcessedFileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public bool Success { get; set; }
    }
}