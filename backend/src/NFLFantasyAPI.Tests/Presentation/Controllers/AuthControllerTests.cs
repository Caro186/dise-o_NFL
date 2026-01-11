using Moq;
using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Presentation.Controllers;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.CrossCutting;
using Xunit;

namespace NFLFantasyAPI.Tests.Presentation.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authController = new AuthController(_authServiceMock.Object);
        }

        #region Register Tests

        [Fact]
        public async Task Register_WhenServiceReturnsOk_ReturnsOk()
        {
            // Arrange
            var dto = new RegistroDto
            {
                Email = "test@example.com",
                Password = "Password123",
                NombreCompleto = "Test User"
            };

            var serviceResult = ServiceResult.Ok(new { mensaje = "Usuario registrado exitosamente" });
            _authServiceMock.Setup(x => x.RegisterAsync(dto))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.Register(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, statusCodeResult.StatusCode);
            _authServiceMock.Verify(x => x.RegisterAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Register_WhenServiceReturnsBadRequest_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegistroDto
            {
                Email = "test@example.com",
                Password = "Password123",
                NombreCompleto = "Test User"
            };

            var serviceResult = ServiceResult.BadRequest("El email ya está registrado");
            _authServiceMock.Setup(x => x.RegisterAsync(dto))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.Register(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task Login_WhenServiceReturnsOk_ReturnsOk()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var loginResponse = new LoginResponseDto
            {
                Status = "ok",
                Token = "test-token",
                TokenExpiracion = DateTime.UtcNow.AddHours(12).ToString("o"),
                Usuario = new UsuarioDto
                {
                    Id = 1,
                    Email = dto.Email,
                    NombreCompleto = "Test User"
                }
            };

            var serviceResult = ServiceResult.Ok(loginResponse);
            _authServiceMock.Setup(x => x.LoginAsync(dto))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.Login(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, statusCodeResult.StatusCode);
            _authServiceMock.Verify(x => x.LoginAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Login_WhenServiceReturnsBadRequest_ReturnsBadRequest()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var serviceResult = ServiceResult.BadRequest("Credenciales inválidas");
            _authServiceMock.Setup(x => x.LoginAsync(dto))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.Login(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        #endregion

        #region Desbloquear Tests

        [Fact]
        public async Task Desbloquear_WhenServiceReturnsOk_ReturnsOk()
        {
            // Arrange
            var dto = new DesbloquearCuentaDto
            {
                Email = "test@example.com"
            };

            var serviceResult = ServiceResult.Ok(new { mensaje = "Cuenta desbloqueada exitosamente" });
            _authServiceMock.Setup(x => x.DesbloquearCuentaAsync(dto.Email))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.Desbloquear(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, statusCodeResult.StatusCode);
            _authServiceMock.Verify(x => x.DesbloquearCuentaAsync(dto.Email), Times.Once);
        }

        #endregion

        #region GetUsuarios Tests

        [Fact]
        public async Task GetUsuarios_WhenServiceReturnsOk_ReturnsOk()
        {
            // Arrange
            var usuarios = new List<UsuarioResponseDto>
            {
                new UsuarioResponseDto
                {
                    Id = 1,
                    Email = "test1@example.com",
                    NombreCompleto = "Test User 1"
                },
                new UsuarioResponseDto
                {
                    Id = 2,
                    Email = "test2@example.com",
                    NombreCompleto = "Test User 2"
                }
            };

            var serviceResult = ServiceResult.Ok(usuarios);
            _authServiceMock.Setup(x => x.GetUsuariosAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.GetUsuarios();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, statusCodeResult.StatusCode);
            _authServiceMock.Verify(x => x.GetUsuariosAsync(), Times.Once);
        }

        #endregion

        #region GetUsuario Tests

        [Fact]
        public async Task GetUsuario_WhenServiceReturnsOk_ReturnsOk()
        {
            // Arrange
            var id = 1;
            var usuario = new UsuarioResponseDto
            {
                Id = id,
                Email = "test@example.com",
                NombreCompleto = "Test User"
            };

            var serviceResult = ServiceResult.Ok(usuario);
            _authServiceMock.Setup(x => x.GetUsuarioAsync(id))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.GetUsuario(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, statusCodeResult.StatusCode);
            _authServiceMock.Verify(x => x.GetUsuarioAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetUsuario_WhenServiceReturnsBadRequest_ReturnsBadRequest()
        {
            // Arrange
            var id = 999;

            var serviceResult = ServiceResult.BadRequest("Usuario no encontrado");
            _authServiceMock.Setup(x => x.GetUsuarioAsync(id))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.GetUsuario(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        #endregion

        #region DeleteUsuario Tests

        [Fact]
        public async Task DeleteUsuario_WhenServiceReturnsOk_ReturnsOk()
        {
            // Arrange
            var id = 1;

            var serviceResult = ServiceResult.Ok(new { mensaje = "Usuario eliminado exitosamente" });
            _authServiceMock.Setup(x => x.DeleteUsuarioAsync(id))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.DeleteUsuario(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, statusCodeResult.StatusCode);
            _authServiceMock.Verify(x => x.DeleteUsuarioAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteUsuario_WhenServiceReturnsBadRequest_ReturnsBadRequest()
        {
            // Arrange
            var id = 999;

            var serviceResult = ServiceResult.BadRequest("Usuario no encontrado");
            _authServiceMock.Setup(x => x.DeleteUsuarioAsync(id))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _authController.DeleteUsuario(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        #endregion
    }
}

