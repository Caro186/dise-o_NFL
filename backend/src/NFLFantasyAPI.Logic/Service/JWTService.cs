using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NFLFantasyAPI.Persistence.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NFLFantasyAPI.Logic.Service
{
    /// <summary>
    /// Servicio para generar y validar tokens JWT
    /// </summary>
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario);
        ClaimsPrincipal? ValidateToken(string token);
    }

    /// <summary>
    /// Implementación del servicio JWT
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Genera un token JWT para el usuario con expiración de 12 horas
        /// </summary>
        /// <param name="usuario">Usuario para el que se genera el token</param>
        /// <returns>Token JWT como string</returns>
        public string GenerateToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer no configurado");
            var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience no configurado");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Name, usuario.NombreCompleto),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", usuario.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12), // Expiración de 12 horas
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            _logger.LogInformation("Token JWT generado para usuario {Email}, expira en 12 horas", usuario.Email);
            
            return tokenString;
        }

        /// <summary>
        /// Valida un token JWT y retorna los claims si es válido
        /// </summary>
        /// <param name="token">Token JWT a validar</param>
        /// <returns>ClaimsPrincipal si el token es válido, null si no lo es</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer no configurado");
                var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience no configurado");

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token JWT inválido o expirado");
                return null;
            }
        }
    }
}
