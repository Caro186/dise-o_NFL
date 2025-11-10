using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.CrossCutting.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NFLFantasyAPI.Logic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUsuarioRepository usuarioRepo,
            ILogger<AuthService> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _usuarioRepo = usuarioRepo;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ServiceResult> RegisterAsync(RegistroDto dto)
        {
            try
            {
                if (await _usuarioRepo.ExistsByEmailAsync(dto.Email))
                    return ServiceResult.BadRequest("El email ya está registrado");

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var usuario = new Usuario
                {
                    Email = dto.Email,
                    Password = passwordHash,
                    NombreCompleto = dto.NombreCompleto,
                    FechaRegistro = DateTime.UtcNow,
                    EstadoCuenta = "Activa",
                    IntentosFailidos = 0,
                    Rol = "Usuario"
                };

                await _usuarioRepo.AddAsync(usuario);
                await _usuarioRepo.SaveChangesAsync();

                return ServiceResult.Ok(new
                {
                    mensaje = "Usuario registrado exitosamente",
                    usuario = new UsuarioDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        FechaRegistro = usuario.FechaRegistro,
                        Rol = usuario.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> LoginAsync(LoginDto dto)
        {
            try
            {
                var usuario = await _usuarioRepo.GetByEmailAsync(dto.Email);
                if (usuario == null)
                    return ServiceResult.BadRequest("Credenciales inválidas");

                if (usuario.EstadoCuenta != "Activa")
                    return ServiceResult.BadRequest("Cuenta bloqueada. Contacta al administrador.");

                bool passwordValido = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password);

                if (!passwordValido)
                {
                    usuario.IntentosFailidos++;
                    usuario.FechaUltimoIntentoFallido = DateTime.UtcNow;

                    if (usuario.IntentosFailidos >= 5)
                    {
                        usuario.EstadoCuenta = "Bloqueada";
                        usuario.FechaBloqueo = DateTime.UtcNow;
                    }

                    await _usuarioRepo.UpdateAsync(usuario);
                    await _usuarioRepo.SaveChangesAsync();

                    return ServiceResult.BadRequest("Credenciales inválidas");
                }

                usuario.IntentosFailidos = 0;
                usuario.FechaUltimoIntentoFallido = null;
                usuario.UltimaActividad = DateTime.UtcNow;
                await _usuarioRepo.UpdateAsync(usuario);
                await _usuarioRepo.SaveChangesAsync();

                var token = GenerateJwtToken(usuario);
                var expiracion = DateTime.UtcNow.AddHours(12);

                return ServiceResult.Ok(new LoginResponseDto
                {
                    Status = "ok",
                    Token = token,
                    TokenExpiracion = expiracion.ToString("o"),
                    Usuario = new UsuarioDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        FechaRegistro = usuario.FechaRegistro,
                        Rol = usuario.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> DesbloquearCuentaAsync(string email)
        {
            try
            {
                var usuario = await _usuarioRepo.GetByEmailAsync(email);
                if (usuario == null)
                    return ServiceResult.BadRequest("Usuario no encontrado");

                usuario.EstadoCuenta = "Activa";
                usuario.IntentosFailidos = 0;
                usuario.FechaUltimoIntentoFallido = null;
                usuario.FechaBloqueo = null;
                await _usuarioRepo.UpdateAsync(usuario);
                await _usuarioRepo.SaveChangesAsync();

                return ServiceResult.Ok(new { mensaje = "Cuenta desbloqueada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desbloquear cuenta");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> GetUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuarioRepo.GetAllAsync();
                var data = usuarios.Select(u => new UsuarioResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    NombreCompleto = u.NombreCompleto,
                    FechaRegistro = u.FechaRegistro
                }).ToList();

                return ServiceResult.Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> GetUsuarioAsync(int id)
        {
            try
            {
                var usuario = await _usuarioRepo.GetByIdAsync(id);
                if (usuario == null)
                    return ServiceResult.BadRequest("Usuario no encontrado");

                var data = new UsuarioResponseDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    FechaRegistro = usuario.FechaRegistro
                };

                return ServiceResult.Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> DeleteUsuarioAsync(int id)
        {
            try
            {
                var usuario = await _usuarioRepo.GetByIdAsync(id);
                if (usuario == null)
                    return ServiceResult.BadRequest("Usuario no encontrado");

                await _usuarioRepo.DeleteAsync(usuario);
                await _usuarioRepo.SaveChangesAsync();

                return ServiceResult.Ok(new { mensaje = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
