using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace NFLFantasyAPI.Logic.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUsuarioRepository usuarioRepo, IJwtService jwtService, ILogger<AuthService> logger)
        {
            _usuarioRepo = usuarioRepo;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ServiceResult> RegisterAsync(RegistroDto dto)
        {
            if (await _usuarioRepo.ExistsByEmailAsync(dto.Email))
                return ServiceResult.BadRequest("El email ya está registrado");

            var usuario = new Usuario
            {
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                NombreCompleto = dto.NombreCompleto,
                FechaRegistro = DateTime.UtcNow,
                EstadoCuenta = "Activa"
            };

            await _usuarioRepo.AddAsync(usuario);
            await _usuarioRepo.SaveChangesAsync();

            _logger.LogInformation("Usuario registrado: {Email}", usuario.Email);

            return ServiceResult.Ok(new UsuarioResponseDto
            {
                Id = usuario.Id,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                FechaRegistro = usuario.FechaRegistro
            });
        }

        public async Task<ServiceResult> LoginAsync(LoginDto dto)
        {
            var usuario = await _usuarioRepo.GetByEmailAsync(dto.Email);

            if (usuario == null)
            {
                _logger.LogWarning("Login fallido: usuario no encontrado {Email}", dto.Email);
                return ServiceResult.BadRequest("Email o contraseña incorrectos");
            }

            if (usuario.EstadoCuenta == "Bloqueada")
                return ServiceResult.BadRequest("Tu cuenta está bloqueada. Contacta al administrador.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password))
            {
                usuario.IntentosFailidos++;
                usuario.FechaUltimoIntentoFallido = DateTime.UtcNow;

                if (usuario.IntentosFailidos >= 5)
                {
                    usuario.EstadoCuenta = "Bloqueada";
                    usuario.FechaBloqueo = DateTime.UtcNow;
                    _logger.LogWarning("Cuenta bloqueada: {Email}", usuario.Email);
                }

                await _usuarioRepo.SaveChangesAsync();
                return ServiceResult.BadRequest("Email o contraseña incorrectos");
            }

            // Éxito
            usuario.IntentosFailidos = 0;
            usuario.FechaUltimoIntentoFallido = null;
            usuario.UltimaActividad = DateTime.UtcNow;
            await _usuarioRepo.SaveChangesAsync();

            var token = _jwtService.GenerateToken(usuario);
            var tokenExpiracion = DateTime.UtcNow.AddHours(12);

            _logger.LogInformation("Login exitoso: {Email}", usuario.Email);

            return ServiceResult.Ok(new LoginResponseDto
            {
                Status = "ok",
                Usuario = new UsuarioResponseDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    FechaRegistro = usuario.FechaRegistro
                },
                Token = token,
                TokenExpiracion = tokenExpiracion
            });
        }

        public async Task<ServiceResult> DesbloquearCuentaAsync(string email)
        {
            var usuario = await _usuarioRepo.GetByEmailAsync(email);

            if (usuario == null)
                return ServiceResult.BadRequest("Usuario no encontrado");

            usuario.EstadoCuenta = "Activa";
            usuario.IntentosFailidos = 0;
            usuario.FechaUltimoIntentoFallido = null;
            usuario.FechaBloqueo = null;

            await _usuarioRepo.SaveChangesAsync();

            _logger.LogInformation("Cuenta desbloqueada: {Email}", email);
            return ServiceResult.Ok("Cuenta desbloqueada exitosamente");
        }

        public async Task<ServiceResult> GetUsuariosAsync()
        {
            var usuarios = await _usuarioRepo.GetAllAsync();

            var lista = usuarios.Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                NombreCompleto = u.NombreCompleto,
                FechaRegistro = u.FechaRegistro
            }).ToList();

            return ServiceResult.Ok(lista);
        }

        public async Task<ServiceResult> GetUsuarioAsync(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);

            if (usuario == null)
                return ServiceResult.BadRequest("Usuario no encontrado");

            var dto = new UsuarioResponseDto
            {
                Id = usuario.Id,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                FechaRegistro = usuario.FechaRegistro
            };

            return ServiceResult.Ok(dto);
        }

        public async Task<ServiceResult> DeleteUsuarioAsync(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);

            if (usuario == null)
                return ServiceResult.BadRequest("Usuario no encontrado");

            _usuarioRepo.Remove(usuario);
            await _usuarioRepo.SaveChangesAsync();

            _logger.LogInformation("Usuario eliminado: {Id}", id);
            return ServiceResult.Ok("Usuario eliminado exitosamente");
        }

    }
}
