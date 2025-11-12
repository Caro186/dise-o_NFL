namespace NFLFantasyAPI.CrossCutting.Configuration
{
    /// <summary>
    /// Configuración de JWT para autenticación
    /// </summary>
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}

