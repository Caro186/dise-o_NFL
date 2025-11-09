/*

using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NFLFantasyAPI.Controllers;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.DTOs;
using NFLFantasyAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
namespace NFLFantasyAPI.Tests.Integration
{
    /// <summary>
    /// Tests de integración para el flujo completo de autenticación
    /// </summary>
    public class AuthenticationIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthController _controller;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly Mock<IJwtService> _jwtServiceMock;

        public AuthenticationIntegrationTests()
        {
            // Configurar base de datos en memoria
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<AuthController>>();
            _jwtServiceMock = new Mock<IJwtService>();
            
            // Configurar mock de JWT para retornar un token fake
            _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<Usuario>()))
                .Returns("fake-jwt-token");

            _controller = new AuthController(_context, _loggerMock.Object, _jwtServiceMock.Object);
        }

        [Fact]
        public async Task FlujoCopleto_RegistroYLoginExitoso()
        {
            // Arrange - Registro
            var registroDto = new RegistroDto
            {
                Email = "test@example.com",
                Password = "Password123",
                NombreCompleto = "Test User"
            };

            // Act - Registrar usuario
            var registroResult = await _controller.Register(registroDto);

            // Assert - Registro exitoso
            var registroOkResult = Assert.IsType<OkObjectResult>(registroResult);
            Assert.NotNull(registroOkResult.Value);

            // Act - Login con credenciales correctas
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var loginResult = await _controller.Login(loginDto);

            // Assert - Login exitoso
            var loginOkResult = Assert.IsType<OkObjectResult>(loginResult);
            var loginResponse = Assert.IsType<LoginResponseDto>(loginOkResult.Value);
            Assert.Equal("ok", loginResponse.Status);
            Assert.NotNull(loginResponse.Usuario);
            Assert.NotNull(loginResponse.Token);
            Assert.Equal("test@example.com", loginResponse.Usuario.Email);
        }

        [Fact]
        public async Task Login_ConCredencialesIncorrectas_RetornaUnauthorized()
        {
            // Arrange - Crear usuario
            var usuario = new Usuario
            {
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                NombreCompleto = "Test User",
                EstadoCuenta = "Activa",
                IntentosFailidos = 0
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Act - Login con contraseña incorrecta
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDto>(unauthorizedResult.Value);
            Assert.Equal("Email o contraseña incorrectos", errorResponse.Mensaje);
        }

        [Fact]
        public async Task Login_Despues5IntentosFallidos_BloqueaCuenta()
        {
            // Arrange - Crear usuario
            var usuario = new Usuario
            {
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                NombreCompleto = "Test User",
                EstadoCuenta = "Activa",
                IntentosFailidos = 0
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            // Act - Hacer 5 intentos fallidos
            for (int i = 0; i < 5; i++)
            {
                await _controller.Login(loginDto);
            }

            // Assert - Verificar que la cuenta está bloqueada
            var usuarioActualizado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(usuarioActualizado);
            Assert.Equal("Bloqueada", usuarioActualizado.EstadoCuenta);
            Assert.Equal(5, usuarioActualizado.IntentosFailidos);
            Assert.NotNull(usuarioActualizado.FechaBloqueo);
        }

        [Fact]
        public async Task Login_ConCuentaBloqueada_RetornaForbidden()
        {
            // Arrange - Crear usuario bloqueado
            var usuario = new Usuario
            {
                Email = "blocked@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                NombreCompleto = "Blocked User",
                EstadoCuenta = "Bloqueada",
                IntentosFailidos = 5,
                FechaBloqueo = DateTime.UtcNow
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Act - Intentar login
            var loginDto = new LoginDto
            {
                Email = "blocked@example.com",
                Password = "Password123"
            };

            var result = await _controller.Login(loginDto);

            // Assert
            var forbiddenResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbiddenResult.StatusCode);
        }

        [Fact]
        public async Task Login_ExitosoResetea_IntentosFailidos()
        {
            // Arrange - Crear usuario con intentos fallidos previos
            var usuario = new Usuario
            {
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                NombreCompleto = "Test User",
                EstadoCuenta = "Activa",
                IntentosFailidos = 3,
                FechaUltimoIntentoFallido = DateTime.UtcNow
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Act - Login exitoso
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            await _controller.Login(loginDto);

            // Assert - Verificar que se resetearon los intentos
            var usuarioActualizado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(usuarioActualizado);
            Assert.Equal(0, usuarioActualizado.IntentosFailidos);
            Assert.Null(usuarioActualizado.FechaUltimoIntentoFallido);
            Assert.NotNull(usuarioActualizado.UltimaActividad);
        }

        [Fact]
        public async Task Login_ExitosoActualiza_UltimaActividad()
        {
            // Arrange - Crear usuario
            var usuario = new Usuario
            {
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                NombreCompleto = "Test User",
                EstadoCuenta = "Activa",
                IntentosFailidos = 0
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var tiempoAntes = DateTime.UtcNow;

            // Act - Login
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            await _controller.Login(loginDto);

            // Assert
            var usuarioActualizado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(usuarioActualizado);
            Assert.NotNull(usuarioActualizado.UltimaActividad);
            Assert.True(usuarioActualizado.UltimaActividad >= tiempoAntes);
        }

        [Fact]
        public async Task Registro_ConEmailDuplicado_RetornaBadRequest()
        {
            // Arrange - Crear primer usuario
            var primerUsuario = new Usuario
            {
                Email = "duplicate@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                NombreCompleto = "First User",
                EstadoCuenta = "Activa"
            };
            _context.Usuarios.Add(primerUsuario);
            await _context.SaveChangesAsync();

            // Act - Intentar registrar con mismo email
            var registroDto = new RegistroDto
            {
                Email = "duplicate@example.com",
                Password = "Password456",
                NombreCompleto = "Second User"
            };

            var result = await _controller.Register(registroDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDto>(badRequestResult.Value);
            Assert.Equal("El email ya está registrado", errorResponse.Mensaje);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}

*/