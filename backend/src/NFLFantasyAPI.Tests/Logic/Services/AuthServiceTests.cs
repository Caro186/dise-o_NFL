using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.Logic.Services;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.CrossCutting.Configuration;
using Xunit;

namespace NFLFantasyAPI.Tests.Logic.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _usuarioRepoMock = new Mock<IUsuarioRepository>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            
            _jwtSettings = Options.Create(new JwtSettings
            {
                Secret = "TestSecretKeyThatIsAtLeast32CharactersLongForJWT",
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            });

            _authService = new AuthService(
                _usuarioRepoMock.Object,
                _loggerMock.Object,
                _jwtSettings);
        }

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegistroDto
            {
                Email = "test@example.com",
                Password = "Password123",
                NombreCompleto = "Test User"
            };

            _usuarioRepoMock.Setup(x => x.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
            _usuarioRepoMock.Verify(x => x.ExistsByEmailAsync(dto.Email), Times.Once);
            _usuarioRepoMock.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenValidData_ReturnsOkWithUsuario()
        {
            // Arrange
            var dto = new RegistroDto
            {
                Email = "test@example.com",
                Password = "Password123",
                NombreCompleto = "Test User"
            };

            _usuarioRepoMock.Setup(x => x.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);
            _usuarioRepoMock.Setup(x => x.AddAsync(It.IsAny<Usuario>()))
                .Returns(Task.CompletedTask);
            _usuarioRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _usuarioRepoMock.Verify(x => x.ExistsByEmailAsync(dto.Email), Times.Once);
            _usuarioRepoMock.Verify(x => x.AddAsync(It.Is<Usuario>(u => 
                u.Email == dto.Email && 
                u.NombreCompleto == dto.NombreCompleto &&
                u.EstadoCuenta == "Activa" &&
                u.Rol == "Usuario"
            )), Times.Once);
            _usuarioRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WhenExceptionThrown_ReturnsError()
        {
            // Arrange
            var dto = new RegistroDto
            {
                Email = "test@example.com",
                Password = "Password123",
                NombreCompleto = "Test User"
            };

            _usuarioRepoMock.Setup(x => x.ExistsByEmailAsync(dto.Email))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.Equal(500, result.StatusCode);
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WhenUserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            _usuarioRepoMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync((Usuario?)null);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
            _usuarioRepoMock.Verify(x => x.GetByEmailAsync(dto.Email), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenAccountBlocked_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var usuario = new Usuario
            {
                Id = 1,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                EstadoCuenta = "Bloqueada"
            };

            _usuarioRepoMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(usuario);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_WhenInvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var usuario = new Usuario
            {
                Id = 1,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                EstadoCuenta = "Activa",
                IntentosFailidos = 0
            };

            _usuarioRepoMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(usuario);
            _usuarioRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Usuario>()))
                .Returns(Task.CompletedTask);
            _usuarioRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
            _usuarioRepoMock.Verify(x => x.UpdateAsync(It.Is<Usuario>(u => 
                u.IntentosFailidos == 1
            )), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenInvalidPassword5Times_BlocksAccount()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var usuario = new Usuario
            {
                Id = 1,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                EstadoCuenta = "Activa",
                IntentosFailidos = 4
            };

            _usuarioRepoMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(usuario);
            _usuarioRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Usuario>()))
                .Returns(Task.CompletedTask);
            _usuarioRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
            _usuarioRepoMock.Verify(x => x.UpdateAsync(It.Is<Usuario>(u => 
                u.IntentosFailidos == 5 &&
                u.EstadoCuenta == "Bloqueada"
            )), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var usuario = new Usuario
            {
                Id = 1,
                Email = dto.Email,
                Password = passwordHash,
                NombreCompleto = "Test User",
                EstadoCuenta = "Activa",
                IntentosFailidos = 0,
                Rol = "Usuario"
            };

            _usuarioRepoMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(usuario);
            _usuarioRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Usuario>()))
                .Returns(Task.CompletedTask);
            _usuarioRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            _usuarioRepoMock.Verify(x => x.UpdateAsync(It.Is<Usuario>(u => 
                u.IntentosFailidos == 0
            )), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenExceptionThrown_ReturnsError()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            _usuarioRepoMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Equal(500, result.StatusCode);
        }

        #endregion

        #region DesbloquearCuentaAsync Tests

        [Fact]
        public async Task DesbloquearCuentaAsync_WhenUserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var email = "test@example.com";

            _usuarioRepoMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((Usuario?)null);

            // Act
            var result = await _authService.DesbloquearCuentaAsync(email);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task DesbloquearCuentaAsync_WhenValidUser_UnblocksAccount()
        {
            // Arrange
            var email = "test@example.com";
            var usuario = new Usuario
            {
                Id = 1,
                Email = email,
                EstadoCuenta = "Bloqueada",
                IntentosFailidos = 5,
                FechaBloqueo = DateTime.UtcNow
            };

            _usuarioRepoMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(usuario);
            _usuarioRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Usuario>()))
                .Returns(Task.CompletedTask);
            _usuarioRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.DesbloquearCuentaAsync(email);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _usuarioRepoMock.Verify(x => x.UpdateAsync(It.Is<Usuario>(u => 
                u.EstadoCuenta == "Activa" &&
                u.IntentosFailidos == 0 &&
                u.FechaBloqueo == null
            )), Times.Once);
        }

        #endregion

        #region GetUsuarioAsync Tests

        [Fact]
        public async Task GetUsuarioAsync_WhenUserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;

            _usuarioRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Usuario?)null);

            // Act
            var result = await _authService.GetUsuarioAsync(id);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetUsuarioAsync_WhenUserExists_ReturnsOk()
        {
            // Arrange
            var id = 1;
            var usuario = new Usuario
            {
                Id = id,
                Email = "test@example.com",
                NombreCompleto = "Test User",
                FechaRegistro = DateTime.UtcNow
            };

            _usuarioRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(usuario);

            // Act
            var result = await _authService.GetUsuarioAsync(id);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        #endregion

        #region DeleteUsuarioAsync Tests

        [Fact]
        public async Task DeleteUsuarioAsync_WhenUserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;

            _usuarioRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Usuario?)null);

            // Act
            var result = await _authService.DeleteUsuarioAsync(id);

            // Assert
            Assert.Equal(400, result.StatusCode);
            _usuarioRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUsuarioAsync_WhenUserExists_DeletesUser()
        {
            // Arrange
            var id = 1;
            var usuario = new Usuario
            {
                Id = id,
                Email = "test@example.com"
            };

            _usuarioRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(usuario);
            _usuarioRepoMock.Setup(x => x.DeleteAsync(It.IsAny<Usuario>()))
                .Returns(Task.CompletedTask);
            _usuarioRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.DeleteUsuarioAsync(id);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _usuarioRepoMock.Verify(x => x.DeleteAsync(usuario), Times.Once);
            _usuarioRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion
    }
}

