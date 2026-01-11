using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using NFLFantasyAPI.Logic.Services;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Validators;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.CrossCutting.Configuration;
using Xunit;

namespace NFLFantasyAPI.Tests.Logic.Services
{
    public class LigaServiceTests
    {
        private readonly Mock<ILigaRepository> _ligaRepoMock;
        private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
        private readonly Mock<IEquipoFantasyRepository> _equipoFantasyRepoMock;
        private readonly Mock<ITemporadaRepository> _temporadaRepoMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<ILogger<LigaService>> _loggerMock;
        private readonly IOptions<FileServerSettings> _fileServerSettings;
        private readonly LigaValidator _ligaValidator;
        private readonly EquipoFantasyValidator _equipoFantasyValidator;
        private readonly LigaService _ligaService;

        public LigaServiceTests()
        {
            _ligaRepoMock = new Mock<ILigaRepository>();
            _usuarioRepoMock = new Mock<IUsuarioRepository>();
            _equipoFantasyRepoMock = new Mock<IEquipoFantasyRepository>();
            _temporadaRepoMock = new Mock<ITemporadaRepository>();
            _environmentMock = new Mock<IWebHostEnvironment>();
            _loggerMock = new Mock<ILogger<LigaService>>();

            _fileServerSettings = Options.Create(new FileServerSettings
            {
                BaseUrl = "http://localhost:5000"
            });

            _environmentMock.Setup(x => x.WebRootPath)
                .Returns("wwwroot");

            // Crear instancias de los validadores con los mocks necesarios
            _ligaValidator = new LigaValidator(
                _ligaRepoMock.Object,
                _temporadaRepoMock.Object,
                _usuarioRepoMock.Object);

            _equipoFantasyValidator = new EquipoFantasyValidator(
                _equipoFantasyRepoMock.Object);

            _ligaService = new LigaService(
                _ligaRepoMock.Object,
                _usuarioRepoMock.Object,
                _equipoFantasyRepoMock.Object,
                _environmentMock.Object,
                _loggerMock.Object,
                _fileServerSettings,
                _ligaValidator,
                _equipoFantasyValidator,
                _temporadaRepoMock.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenLigasExist_ReturnsOkWithLigas()
        {
            // Arrange
            var ligas = new List<Liga>
            {
                new Liga
                {
                    IdLiga = 1,
                    NombreLiga = "Liga Test 1",
                    Estado = "Pre-Draft"
                },
                new Liga
                {
                    IdLiga = 2,
                    NombreLiga = "Liga Test 2",
                    Estado = "Activa"
                }
            };

            _ligaRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(ligas);

            // Act
            var result = await _ligaService.GetAllAsync();

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoLigas_ReturnsOkWithEmptyList()
        {
            // Arrange
            _ligaRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Liga>());

            // Act
            var result = await _ligaService.GetAllAsync();

            // Assert
            Assert.Equal(200, result.StatusCode);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenLigaNotFound_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;

            _ligaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Liga?)null);

            // Act
            var result = await _ligaService.GetByIdAsync(id);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenLigaExists_ReturnsOk()
        {
            // Arrange
            var id = 1;
            var liga = new Liga
            {
                IdLiga = id,
                NombreLiga = "Liga Test",
                Estado = "Pre-Draft"
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(liga);

            // Act
            var result = await _ligaService.GetByIdAsync(id);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WhenValidData_CreatesLiga()
        {
            // Arrange
            var dto = new LigaCreateDto
            {
                NombreLiga = "Nueva Liga",
                Descripcion = "Descripción de prueba",
                PasswordHash = "password123",
                IdTemporada = 1,
                CuposTotales = 10,
                ComisionadoId = 1
            };

            // Configurar mocks para validaciones del validador
            var temporada = new Temporada { Id = 1, Nombre = "Temporada 2024" };
            var usuario = new Usuario { Id = 1, Email = "test@test.com" };

            _temporadaRepoMock.Setup(x => x.GetByIdAsync(dto.IdTemporada))
                .ReturnsAsync(temporada);
            _usuarioRepoMock.Setup(x => x.GetByIdAsync(dto.ComisionadoId))
                .ReturnsAsync(usuario);
            _ligaRepoMock.Setup(x => x.ExistsByNombreAsync(dto.NombreLiga, null))
                .ReturnsAsync(false);

            _ligaRepoMock.Setup(x => x.AddAsync(It.IsAny<Liga>()))
                .Returns(Task.CompletedTask);
            _ligaRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _ligaService.CreateAsync(dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _ligaRepoMock.Verify(x => x.AddAsync(It.Is<Liga>(l =>
                l.NombreLiga == dto.NombreLiga &&
                l.Descripcion == dto.Descripcion &&
                l.IdTemporada == dto.IdTemporada &&
                l.CuposTotales == dto.CuposTotales &&
                l.ComisionadoId == dto.ComisionadoId &&
                l.Estado == "Pre-Draft" &&
                l.CuposOcupados == 1
            )), Times.Once);
            _ligaRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenLigaNotFound_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;
            var dto = new LigaCreateDto
            {
                NombreLiga = "Liga Actualizada",
                Descripcion = "Nueva descripción"
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Liga?)null);

            // Act
            var result = await _ligaService.UpdateAsync(id, dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
            _ligaRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Liga>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenLigaExists_UpdatesLiga()
        {
            // Arrange
            var id = 1;
            var liga = new Liga
            {
                IdLiga = id,
                NombreLiga = "Liga Original",
                Descripcion = "Descripción original"
            };

            var dto = new LigaCreateDto
            {
                NombreLiga = "Liga Actualizada",
                Descripcion = "Nueva descripción"
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(liga);
            _ligaRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Liga>()))
                .Returns(Task.CompletedTask);
            _ligaRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _ligaService.UpdateAsync(id, dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _ligaRepoMock.Verify(x => x.UpdateAsync(It.Is<Liga>(l =>
                l.NombreLiga == dto.NombreLiga &&
                l.Descripcion == dto.Descripcion
            )), Times.Once);
            _ligaRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenLigaNotFound_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;

            _ligaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Liga?)null);

            // Act
            var result = await _ligaService.DeleteAsync(id);

            // Assert
            Assert.Equal(400, result.StatusCode);
            _ligaRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Liga>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenLigaExists_DeletesLiga()
        {
            // Arrange
            var id = 1;
            var liga = new Liga
            {
                IdLiga = id,
                NombreLiga = "Liga a Eliminar"
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(liga);
            _ligaRepoMock.Setup(x => x.DeleteAsync(It.IsAny<Liga>()))
                .Returns(Task.CompletedTask);
            _ligaRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _ligaService.DeleteAsync(id);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _ligaRepoMock.Verify(x => x.DeleteAsync(liga), Times.Once);
            _ligaRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region UnirseLigaAsync Tests

        [Fact]
        public async Task UnirseLigaAsync_WhenLigaNotFound_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UnirseLigaDto
            {
                LigaId = 1,
                Password = "password123",
                UsuarioId = 1,
                EquipoId = 1,
                Alias = "Mi Equipo"
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(dto.LigaId))
                .ReturnsAsync((Liga?)null);

            // Act
            var result = await _ligaService.UnirseLigaAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UnirseLigaAsync_WhenInvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UnirseLigaDto
            {
                LigaId = 1,
                Password = "wrongpassword",
                UsuarioId = 1,
                EquipoId = 1,
                Alias = "Mi Equipo"
            };

            var liga = new Liga
            {
                IdLiga = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                CuposTotales = 10,
                CuposOcupados = 5
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(dto.LigaId))
                .ReturnsAsync(liga);

            // Act
            var result = await _ligaService.UnirseLigaAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UnirseLigaAsync_WhenLigaFull_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UnirseLigaDto
            {
                LigaId = 1,
                Password = "password123",
                UsuarioId = 1,
                EquipoId = 1,
                Alias = "Mi Equipo"
            };

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var liga = new Liga
            {
                IdLiga = 1,
                PasswordHash = passwordHash,
                CuposTotales = 10,
                CuposOcupados = 10
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(dto.LigaId))
                .ReturnsAsync(liga);

            // Act
            var result = await _ligaService.UnirseLigaAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UnirseLigaAsync_WhenUsuarioNotFound_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UnirseLigaDto
            {
                LigaId = 1,
                Password = "password123",
                UsuarioId = 1,
                EquipoId = 1,
                Alias = "Mi Equipo"
            };

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var liga = new Liga
            {
                IdLiga = 1,
                PasswordHash = passwordHash,
                CuposTotales = 10,
                CuposOcupados = 5
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(dto.LigaId))
                .ReturnsAsync(liga);
            _usuarioRepoMock.Setup(x => x.GetByIdAsync(dto.UsuarioId))
                .ReturnsAsync((Usuario?)null);

            // Act
            var result = await _ligaService.UnirseLigaAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UnirseLigaAsync_WhenEquipoNotFound_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UnirseLigaDto
            {
                LigaId = 1,
                Password = "password123",
                UsuarioId = 1,
                EquipoId = 1,
                Alias = "Mi Equipo"
            };

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var liga = new Liga
            {
                IdLiga = 1,
                PasswordHash = passwordHash,
                CuposTotales = 10,
                CuposOcupados = 5
            };

            var usuario = new Usuario
            {
                Id = 1,
                Email = "test@example.com"
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(dto.LigaId))
                .ReturnsAsync(liga);
            _usuarioRepoMock.Setup(x => x.GetByIdAsync(dto.UsuarioId))
                .ReturnsAsync(usuario);
            _equipoFantasyRepoMock.Setup(x => x.GetByIdAsync(dto.EquipoId))
                .ReturnsAsync((EquipoFantasy?)null);

            // Act
            var result = await _ligaService.UnirseLigaAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UnirseLigaAsync_WhenEquipoAlreadyInLiga_ReturnsBadRequest()
        {
            // Arrange
            var dto = new UnirseLigaDto
            {
                LigaId = 1,
                Password = "password123",
                UsuarioId = 1,
                EquipoId = 1,
                Alias = "Mi Equipo"
            };

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var liga = new Liga
            {
                IdLiga = 1,
                PasswordHash = passwordHash,
                CuposTotales = 10,
                CuposOcupados = 5
            };

            var usuario = new Usuario
            {
                Id = 1,
                Email = "test@example.com"
            };

            var equipo = new EquipoFantasy
            {
                Id = 1,
                UsuarioId = 1,
                LigaId = 2 // Ya está en otra liga
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(dto.LigaId))
                .ReturnsAsync(liga);
            _usuarioRepoMock.Setup(x => x.GetByIdAsync(dto.UsuarioId))
                .ReturnsAsync(usuario);
            _equipoFantasyRepoMock.Setup(x => x.GetByIdAsync(dto.EquipoId))
                .ReturnsAsync(equipo);

            // Act
            var result = await _ligaService.UnirseLigaAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UnirseLigaAsync_WhenValidData_JoinsLiga()
        {
            // Arrange
            var dto = new UnirseLigaDto
            {
                LigaId = 1,
                Password = "password123",
                UsuarioId = 1,
                EquipoId = 1,
                Alias = "Mi Equipo"
            };

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var liga = new Liga
            {
                IdLiga = 1,
                NombreLiga = "Liga Test",
                PasswordHash = passwordHash,
                CuposTotales = 10,
                CuposOcupados = 5
            };

            var usuario = new Usuario
            {
                Id = 1,
                Email = "test@example.com"
            };

            var equipo = new EquipoFantasy
            {
                Id = 1,
                UsuarioId = 1,
                LigaId = null // No está en ninguna liga
            };

            _ligaRepoMock.Setup(x => x.GetByIdAsync(dto.LigaId))
                .ReturnsAsync(liga);
            _usuarioRepoMock.Setup(x => x.GetByIdAsync(dto.UsuarioId))
                .ReturnsAsync(usuario);
            // Configurar mocks para validadores (el equipo debe obtenerse múltiples veces)
            _equipoFantasyRepoMock.Setup(x => x.GetByIdAsync(dto.EquipoId))
                .ReturnsAsync(equipo);
            _equipoFantasyRepoMock.Setup(x => x.GetByUsuarioIdAsync(dto.UsuarioId))
                .ReturnsAsync(new List<EquipoFantasy>()); // Lista vacía para que no tenga equipo en la liga
            _equipoFantasyRepoMock.Setup(x => x.UpdateAsync(It.IsAny<EquipoFantasy>()))
                .Returns(Task.CompletedTask);
            _ligaRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Liga>()))
                .Returns(Task.CompletedTask);
            _ligaRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _ligaService.UnirseLigaAsync(dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _equipoFantasyRepoMock.Verify(x => x.UpdateAsync(It.Is<EquipoFantasy>(e =>
                e.LigaId == dto.LigaId
            )), Times.Once);
            _ligaRepoMock.Verify(x => x.UpdateAsync(It.Is<Liga>(l =>
                l.CuposOcupados == 6
            )), Times.Once);
        }

        #endregion
    }
}

