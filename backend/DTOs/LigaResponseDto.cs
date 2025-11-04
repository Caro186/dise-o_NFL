namespace NFLFantasyAPI.DTOs
{
    public class LigaResponseDto
    {
        public int IdLiga { get; set; }
        public string? ImagenUrl { get; set; }
        public string NombreLiga { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int IdTemporada { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int CuposTotales { get; set; }
        public int CuposOcupados { get; set; }
        public int CuposDisponibles { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int IdComisionado { get; set; }
        public string NombreComisionado { get; set; } = string.Empty;
        public int IdEquipoComisionado { get; set; }
        public string NombreEquipoComisionado { get; set; } = string.Empty;
    }
}