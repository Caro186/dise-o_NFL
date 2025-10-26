namespace NFLFantasyAPI.DTOs{
    public class LigaResponseDto
    {
        public int IdLiga { get; set; }

        public string? ImagenUrl { get; set; }

        public string NombreLiga { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        // No se envía PasswordHash en la respuesta por seguridad

        public string Temporada { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public int CuposTotales { get; set; }

        public int CuposOcupados { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public int IdCreador { get; set; }
    }
}