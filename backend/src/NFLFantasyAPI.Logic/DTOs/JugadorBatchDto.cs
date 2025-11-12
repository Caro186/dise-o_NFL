using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para un jugador individual en el archivo JSON de batch
    /// </summary>
    public class JugadorBatchItemDto
    {
        [Required(ErrorMessage = "El ID es requerido")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La posición es requerida")]
        [StringLength(50, ErrorMessage = "La posición no puede exceder 50 caracteres")]
        public string Posicion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El equipo NFL es requerido")]
        public int EquipoNFLId { get; set; }

        [StringLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
        public string? ImagenUrl { get; set; }
    }

    /// <summary>
    /// DTO para el array completo de jugadores del archivo JSON
    /// </summary>
    public class JugadorBatchRequestDto
    {
        [Required(ErrorMessage = "La lista de jugadores es requerida")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un jugador")]
        public List<JugadorBatchItemDto> Jugadores { get; set; } = new();
    }

    /// <summary>
    /// DTO para el reporte de resultados del batch
    /// </summary>
    public class JugadorBatchResultDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int TotalProcesados { get; set; }
        public int TotalExitosos { get; set; }
        public int TotalErrores { get; set; }
        public List<JugadorBatchErrorDto> Errores { get; set; } = new();
        public List<JugadorCreatedDto> JugadoresCreados { get; set; } = new();
        public string ArchivoMovidoA { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para errores individuales en el batch
    /// </summary>
    public class JugadorBatchErrorDto
    {
        public int? Id { get; set; }
        public string? Nombre { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para jugadores creados exitosamente
    /// </summary>
    public class JugadorCreatedDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Posicion { get; set; } = string.Empty;
        public string NombreEquipoNFL { get; set; } = string.Empty;
    }

}