using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NFLFantasyAPI.Logic.Services;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.Validators;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.CrossCutting.Configuration;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace NFLFantasyAPI.Tests.Logic.Services
{
    public class JugadorServiceTests
    {
        private readonly Mock<IJugadorRepository> _jugadorRepoMock;
        private readonly Mock<IEquipoNFLRepository> _equipoNFLRepoMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<ILogger<JugadorService>> _loggerMock;
        private readonly IOptions<FileServerSettings> _fileServerSettings;
        private readonly Mock<JugadorValidator> _validatorMock;
        private readonly Mock<IBatchFileProcessingService> _batchFileServiceMock;
        private readonly JugadorService _jugadorService;

        public JugadorServiceTests()
        {
            _jugadorRepoMock = new Mock<IJugadorRepository>();
            _equipoNFLRepoMock = new Mock<IEquipoNFLRepository>();
            _environmentMock = new Mock<IWebHostEnvironment>();
            _loggerMock = new Mock<ILogger<JugadorService>>();
            _batchFileServiceMock = new Mock<IBatchFileProcessingService>();

            _fileServerSettings = Options.Create(new FileServerSettings
            {
                BaseUrl = "http://localhost:5000"
            });

            _environmentMock.Setup(x => x.WebRootPath)
                .Returns("wwwroot");

            // Crear un validador real con mocks de repositorios
            var validatorJugadorRepoMock = new Mock<IJugadorRepository>();
            var validatorEquipoNFLRepoMock = new Mock<IEquipoNFLRepository>();
            var realValidator = new JugadorValidator(validatorJugadorRepoMock.Object, validatorEquipoNFLRepoMock.Object);
            _validatorMock = new Mock<JugadorValidator>(validatorJugadorRepoMock.Object, validatorEquipoNFLRepoMock.Object);
            
            // Configurar el validador real para usar en el servicio
            var validator = new JugadorValidator(_jugadorRepoMock.Object, _equipoNFLRepoMock.Object);

            _jugadorService = new JugadorService(
                _jugadorRepoMock.Object,
                _equipoNFLRepoMock.Object,
                _environmentMock.Object,
                _loggerMock.Object,
                _fileServerSettings,
                validator,
                _batchFileServiceMock.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenJugadoresExist_ReturnsOkWithJugadores()
        {
            // Arrange
            var jugadores = new List<Jugador>
            {
                new Jugador
                {
                    Id = 1,
                    Nombre = "Tom Brady",
                    Posicion = "QB",
                    Estado = "Activo",
                    EquipoNFL = new EquipoNFL { Nombre = "Buccaneers" }
                },
                new Jugador
                {
                    Id = 2,
                    Nombre = "Aaron Rodgers",
                    Posicion = "QB",
                    Estado = "Activo",
                    EquipoNFL = new EquipoNFL { Nombre = "Packers" }
                }
            };

            _jugadorRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(jugadores);

            // Act
            var result = await _jugadorService.GetAllAsync();

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoJugadores_ReturnsOkWithEmptyList()
        {
            // Arrange
            _jugadorRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Jugador>());

            // Act
            var result = await _jugadorService.GetAllAsync();

            // Assert
            Assert.Equal(200, result.StatusCode);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenJugadorNotFound_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Jugador?)null);

            // Act
            var result = await _jugadorService.GetByIdAsync(id);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenJugadorExists_ReturnsOk()
        {
            // Arrange
            var id = 1;
            var jugador = new Jugador
            {
                Id = id,
                Nombre = "Tom Brady",
                Posicion = "QB",
                Estado = "Activo",
                EquipoNFL = new EquipoNFL
                {
                    Id = 1,
                    Nombre = "Buccaneers",
                    Ciudad = "Tampa Bay"
                }
            };

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(jugador);

            // Act
            var result = await _jugadorService.GetByIdAsync(id);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WhenEquipoNFLNotFound_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CrearJugadorDto
            {
                Nombre = "Tom Brady",
                Posicion = "QB",
                EquipoNFLId = 999
            };

            _jugadorRepoMock.Setup(x => x.EquipoExistsAsync(dto.EquipoNFLId))
                .ReturnsAsync(false);

            // Act
            var result = await _jugadorService.CreateAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_WhenJugadorDuplicado_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CrearJugadorDto
            {
                Nombre = "Tom Brady",
                Posicion = "QB",
                EquipoNFLId = 1
            };

            _jugadorRepoMock.Setup(x => x.EquipoExistsAsync(dto.EquipoNFLId))
                .ReturnsAsync(true);
            _jugadorRepoMock.Setup(x => x.ExistsInEquipoAsync(dto.Nombre, dto.EquipoNFLId))
                .ReturnsAsync(true);

            // Act
            var result = await _jugadorService.CreateAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_WhenValidData_CreatesJugador()
        {
            // Arrange
            var dto = new CrearJugadorDto
            {
                Nombre = "Tom Brady",
                Posicion = "QB",
                EquipoNFLId = 1,
                ImagenUrl = "https://example.com/image.jpg"
            };

            _jugadorRepoMock.Setup(x => x.EquipoExistsAsync(dto.EquipoNFLId))
                .ReturnsAsync(true);
            _jugadorRepoMock.Setup(x => x.ExistsInEquipoAsync(dto.Nombre, dto.EquipoNFLId))
                .ReturnsAsync(false);
            _jugadorRepoMock.Setup(x => x.AddAsync(It.IsAny<Jugador>()))
                .Returns(Task.CompletedTask);
            _jugadorRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _jugadorService.CreateAsync(dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _jugadorRepoMock.Verify(x => x.AddAsync(It.Is<Jugador>(j =>
                j.Nombre == dto.Nombre &&
                j.Posicion == dto.Posicion &&
                j.EquipoNFLId == dto.EquipoNFLId &&
                j.Estado == "Activo"
            )), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenInvalidPosition_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CrearJugadorDto
            {
                Nombre = "Tom Brady",
                Posicion = "INVALID",
                EquipoNFLId = 1
            };

            _jugadorRepoMock.Setup(x => x.EquipoExistsAsync(dto.EquipoNFLId))
                .ReturnsAsync(true);
            _jugadorRepoMock.Setup(x => x.ExistsInEquipoAsync(dto.Nombre, dto.EquipoNFLId))
                .ReturnsAsync(false);

            // Act
            var result = await _jugadorService.CreateAsync(dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenJugadorNotFound_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;
            var dto = new ActualizarJugadorDto
            {
                Nombre = "Tom Brady Updated"
            };

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Jugador?)null);

            // Act
            var result = await _jugadorService.UpdateAsync(id, dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_WhenJugadorExists_UpdatesJugador()
        {
            // Arrange
            var id = 1;
            var jugador = new Jugador
            {
                Id = id,
                Nombre = "Tom Brady",
                Posicion = "QB",
                EquipoNFLId = 1,
                Estado = "Activo"
            };

            var dto = new ActualizarJugadorDto
            {
                Nombre = "Tom Brady Updated",
                Posicion = "QB"
            };

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(jugador);
            _jugadorRepoMock.Setup(x => x.ExistsInEquipoAsync(dto.Nombre!, jugador.EquipoNFLId))
                .ReturnsAsync(false);
            _jugadorRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Jugador>()))
                .Returns(Task.CompletedTask);
            _jugadorRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _jugadorService.UpdateAsync(id, dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _jugadorRepoMock.Verify(x => x.UpdateAsync(It.Is<Jugador>(j =>
                j.Nombre == dto.Nombre &&
                j.Posicion == dto.Posicion
            )), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenJugadorNotFound_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Jugador?)null);

            // Act
            var result = await _jugadorService.DeleteAsync(id, false);

            // Assert
            Assert.Equal(400, result.StatusCode);
            _jugadorRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Jugador>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenPermanent_DeletesJugador()
        {
            // Arrange
            var id = 1;
            var jugador = new Jugador
            {
                Id = id,
                Nombre = "Tom Brady"
            };

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(jugador);
            _jugadorRepoMock.Setup(x => x.DeleteAsync(It.IsAny<Jugador>()))
                .Returns(Task.CompletedTask);
            _jugadorRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _jugadorService.DeleteAsync(id, true);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _jugadorRepoMock.Verify(x => x.DeleteAsync(jugador), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotPermanent_DeactivatesJugador()
        {
            // Arrange
            var id = 1;
            var jugador = new Jugador
            {
                Id = id,
                Nombre = "Tom Brady",
                Estado = "Activo"
            };

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(jugador);
            _jugadorRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Jugador>()))
                .Returns(Task.CompletedTask);
            _jugadorRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _jugadorService.DeleteAsync(id, false);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _jugadorRepoMock.Verify(x => x.UpdateAsync(It.Is<Jugador>(j =>
                j.Estado == "Inactivo"
            )), Times.Once);
            _jugadorRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Jugador>()), Times.Never);
        }

        #endregion

        #region GetByEquipoAsync Tests

        [Fact]
        public async Task GetByEquipoAsync_WhenEquipoNotFound_ReturnsBadRequest()
        {
            // Arrange
            var equipoId = 999;

            _jugadorRepoMock.Setup(x => x.EquipoExistsAsync(equipoId))
                .ReturnsAsync(false);

            // Act
            var result = await _jugadorService.GetByEquipoAsync(equipoId);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetByEquipoAsync_WhenEquipoExists_ReturnsOk()
        {
            // Arrange
            var equipoId = 1;
            var jugadores = new List<Jugador>
            {
                new Jugador
                {
                    Id = 1,
                    Nombre = "Tom Brady",
                    Posicion = "QB",
                    EquipoNFL = new EquipoNFL { Nombre = "Buccaneers" }
                }
            };

            _jugadorRepoMock.Setup(x => x.EquipoExistsAsync(equipoId))
                .ReturnsAsync(true);
            _jugadorRepoMock.Setup(x => x.GetByEquipoAsync(equipoId))
                .ReturnsAsync(jugadores);

            // Act
            var result = await _jugadorService.GetByEquipoAsync(equipoId);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        #endregion

        #region GetByPosicionAsync Tests

        [Fact]
        public async Task GetByPosicionAsync_WhenJugadoresExist_ReturnsOk()
        {
            // Arrange
            var posicion = "QB";
            var jugadores = new List<Jugador>
            {
                new Jugador
                {
                    Id = 1,
                    Nombre = "Tom Brady",
                    Posicion = posicion,
                    EquipoNFL = new EquipoNFL { Nombre = "Buccaneers" }
                }
            };

            _jugadorRepoMock.Setup(x => x.GetByPosicionAsync(posicion))
                .ReturnsAsync(jugadores);

            // Act
            var result = await _jugadorService.GetByPosicionAsync(posicion);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task UpdateAsync_WhenInvalidEstado_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;
            var jugador = new Jugador
            {
                Id = id,
                Nombre = "Tom Brady",
                Posicion = "QB",
                EquipoNFLId = 1,
                Estado = "Activo"
            };

            var dto = new ActualizarJugadorDto
            {
                Estado = "EstadoInvalido" // Estado inválido
            };

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(jugador);

            // Act
            var result = await _jugadorService.UpdateAsync(id, dto);

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_WhenValidEstado_UpdatesSuccessfully()
        {
            // Arrange
            var id = 1;
            var jugador = new Jugador
            {
                Id = id,
                Nombre = "Tom Brady",
                Posicion = "QB",
                EquipoNFLId = 1,
                Estado = "Activo"
            };

            var dto = new ActualizarJugadorDto
            {
                Estado = "Inactivo" // Estado válido
            };

            _jugadorRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(jugador);
            _jugadorRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Jugador>()))
                .Returns(Task.CompletedTask);
            _jugadorRepoMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _jugadorService.UpdateAsync(id, dto);

            // Assert
            Assert.Equal(200, result.StatusCode);
            _jugadorRepoMock.Verify(x => x.UpdateAsync(It.Is<Jugador>(j =>
                j.Estado == "Inactivo"
            )), Times.Once);
        }

        #endregion

        #region ProcessBatchFileAsync Tests

        [Fact]
        public async Task ProcessBatchFileAsync_WhenFileIsNull_ThrowsInvalidFileException()
        {
            // Arrange
            IFormFile? file = null;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidFileException>(async () =>
                await _jugadorService.ProcessBatchFileAsync(file!));
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenFileExtensionIsInvalid_ThrowsInvalidFileException()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(100);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidFileException>(async () =>
                await _jugadorService.ProcessBatchFileAsync(fileMock.Object));
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenFileIsEmpty_ThrowsInvalidFileException()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            fileMock.Setup(f => f.Length).Returns(0);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidFileException>(async () =>
                await _jugadorService.ProcessBatchFileAsync(fileMock.Object));
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenJsonIsInvalid_ThrowsBatchProcessingException()
        {
            // Arrange
            var invalidJson = "invalid json content";
            var fileContent = Encoding.UTF8.GetBytes(invalidJson);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            _batchFileServiceMock.Setup(s => s.ReadFileContentAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(fileContent);
            _batchFileServiceMock.Setup(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), false, "jugadores"))
                .ReturnsAsync("failed_path.json");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BatchProcessingException>(async () =>
                await _jugadorService.ProcessBatchFileAsync(fileMock.Object));

            Assert.Contains("Error al parsear el archivo JSON", exception.Message);
            _batchFileServiceMock.Verify(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), false, "jugadores"), Times.Once);
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenNoJugadoresInFile_ThrowsBatchProcessingException()
        {
            // Arrange
            var batchRequest = new JugadorBatchRequestDto { Jugadores = new List<JugadorBatchItemDto>() };
            var jsonContent = JsonSerializer.Serialize(batchRequest);
            var fileContent = Encoding.UTF8.GetBytes(jsonContent);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            _batchFileServiceMock.Setup(s => s.ReadFileContentAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(fileContent);
            _batchFileServiceMock.Setup(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), false, "jugadores"))
                .ReturnsAsync("failed_path.json");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BatchProcessingException>(async () =>
                await _jugadorService.ProcessBatchFileAsync(fileMock.Object));

            Assert.Contains("no contiene jugadores válidos", exception.Message);
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenValidationFails_ReturnsResultWithErrors()
        {
            // Arrange
            var batchRequest = new JugadorBatchRequestDto
            {
                Jugadores = new List<JugadorBatchItemDto>
                {
                    new JugadorBatchItemDto
                    {
                        Id = 1,
                        Nombre = "", // Error: nombre vacío
                        Posicion = "INVALID", // Error: posición inválida
                        EquipoNFLId = 1
                    }
                }
            };
            var jsonContent = JsonSerializer.Serialize(batchRequest);
            var fileContent = Encoding.UTF8.GetBytes(jsonContent);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            _batchFileServiceMock.Setup(s => s.ReadFileContentAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(fileContent);
            _batchFileServiceMock.Setup(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), false, "jugadores"))
                .ReturnsAsync("failed_path.json");

            // Configurar repositorios para validación
            _equipoNFLRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<EquipoNFL>());
            _jugadorRepoMock.Setup(r => r.GetByEquipoAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Jugador>());

            // Act
            var result = await _jugadorService.ProcessBatchFileAsync(fileMock.Object);

            // Assert
            Assert.False(result.Exito);
            Assert.True(result.TotalErrores > 0);
            Assert.NotEmpty(result.Errores);
            Assert.Contains(result.Errores, e => e.Error.Contains("nombre") || e.Error.Contains("posición"));
            _batchFileServiceMock.Verify(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), false, "jugadores"), Times.Once);
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenValidJugadores_CreatesJugadoresReusingManualCreation()
        {
            // Arrange
            var batchRequest = new JugadorBatchRequestDto
            {
                Jugadores = new List<JugadorBatchItemDto>
                {
                    new JugadorBatchItemDto
                    {
                        Id = 1,
                        Nombre = "Patrick Mahomes",
                        Posicion = "QB",
                        EquipoNFLId = 1,
                        ImagenUrl = "https://example.com/mahomes.jpg"
                    },
                    new JugadorBatchItemDto
                    {
                        Id = 2,
                        Nombre = "Travis Kelce",
                        Posicion = "TE",
                        EquipoNFLId = 1,
                        ImagenUrl = "https://example.com/kelce.jpg"
                    }
                }
            };
            var jsonContent = JsonSerializer.Serialize(batchRequest);
            var fileContent = Encoding.UTF8.GetBytes(jsonContent);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            var equipo = new EquipoNFL { Id = 1, Nombre = "Kansas City Chiefs" };

            _batchFileServiceMock.Setup(s => s.ReadFileContentAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(fileContent);
            _batchFileServiceMock.Setup(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), true, "jugadores"))
                .ReturnsAsync("success_path.json");

            // Configurar repositorios para validación
            _equipoNFLRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<EquipoNFL> { equipo });
            _jugadorRepoMock.Setup(r => r.GetByEquipoAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Jugador>());

            // Configurar transacción
            // Moq puede tener problemas con ValueTask, así que usamos Callback para permitir la ejecución
            var transactionMock = new Mock<IDbContextTransaction>();
            // No configuramos CommitAsync explícitamente - el mock básico permitirá su ejecución
            // El using statement manejará Dispose automáticamente
            _jugadorRepoMock.Setup(r => r.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            // Configurar creación de jugadores (reutiliza CrearJugadorInternoAsync)
            _jugadorRepoMock.Setup(r => r.AddAsync(It.IsAny<Jugador>()))
                .Returns(Task.CompletedTask);
            _jugadorRepoMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _equipoNFLRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(equipo);

            // Act
            var result = await _jugadorService.ProcessBatchFileAsync(fileMock.Object);

            // Assert
            Assert.True(result.Exito);
            Assert.Equal(2, result.TotalProcesados);
            Assert.Equal(2, result.TotalExitosos);
            Assert.Equal(0, result.TotalErrores);
            Assert.Empty(result.Errores);
            Assert.Equal(2, result.JugadoresCreados.Count);

            // Verificar que se llamó AddAsync para cada jugador (reutiliza creación manual)
            _jugadorRepoMock.Verify(r => r.AddAsync(It.Is<Jugador>(j =>
                j.Nombre == "Patrick Mahomes" &&
                j.Posicion == "QB" &&
                j.EquipoNFLId == 1 &&
                j.Estado == "Activo"
            )), Times.Once);

            _jugadorRepoMock.Verify(r => r.AddAsync(It.Is<Jugador>(j =>
                j.Nombre == "Travis Kelce" &&
                j.Posicion == "TE" &&
                j.EquipoNFLId == 1 &&
                j.Estado == "Activo"
            )), Times.Once);

            // Verificar que se guardó el archivo como exitoso
            _batchFileServiceMock.Verify(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), true, "jugadores"), Times.Once);
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenDuplicateInBatch_ReturnsValidationError()
        {
            // Arrange
            var batchRequest = new JugadorBatchRequestDto
            {
                Jugadores = new List<JugadorBatchItemDto>
                {
                    new JugadorBatchItemDto
                    {
                        Id = 1,
                        Nombre = "Patrick Mahomes",
                        Posicion = "QB",
                        EquipoNFLId = 1
                    },
                    new JugadorBatchItemDto
                    {
                        Id = 2,
                        Nombre = "Patrick Mahomes", // Duplicado en el batch
                        Posicion = "QB",
                        EquipoNFLId = 1
                    }
                }
            };
            var jsonContent = JsonSerializer.Serialize(batchRequest);
            var fileContent = Encoding.UTF8.GetBytes(jsonContent);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            var equipo = new EquipoNFL { Id = 1, Nombre = "Kansas City Chiefs" };

            _batchFileServiceMock.Setup(s => s.ReadFileContentAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(fileContent);
            _batchFileServiceMock.Setup(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), false, "jugadores"))
                .ReturnsAsync("failed_path.json");

            _equipoNFLRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<EquipoNFL> { equipo });
            _jugadorRepoMock.Setup(r => r.GetByEquipoAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Jugador>());

            // Act
            var result = await _jugadorService.ProcessBatchFileAsync(fileMock.Object);

            // Assert
            Assert.False(result.Exito);
            Assert.True(result.TotalErrores > 0);
            Assert.Contains(result.Errores, e => 
                e.Error.Contains("duplicado") || e.Error.Contains("Duplicado"));
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenAllOrNothing_FailsAllIfOneInvalid()
        {
            // Arrange - Un jugador válido y uno inválido
            var batchRequest = new JugadorBatchRequestDto
            {
                Jugadores = new List<JugadorBatchItemDto>
                {
                    new JugadorBatchItemDto
                    {
                        Id = 1,
                        Nombre = "Patrick Mahomes",
                        Posicion = "QB",
                        EquipoNFLId = 1
                    },
                    new JugadorBatchItemDto
                    {
                        Id = 2,
                        Nombre = "", // Inválido - nombre vacío
                        Posicion = "QB",
                        EquipoNFLId = 1
                    }
                }
            };
            var jsonContent = JsonSerializer.Serialize(batchRequest);
            var fileContent = Encoding.UTF8.GetBytes(jsonContent);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            var equipo = new EquipoNFL { Id = 1, Nombre = "Kansas City Chiefs" };

            _batchFileServiceMock.Setup(s => s.ReadFileContentAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(fileContent);
            _batchFileServiceMock.Setup(s => s.SaveProcessedFileAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), false, "jugadores"))
                .ReturnsAsync("failed_path.json");

            _equipoNFLRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<EquipoNFL> { equipo });
            _jugadorRepoMock.Setup(r => r.GetByEquipoAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Jugador>());

            // Act
            var result = await _jugadorService.ProcessBatchFileAsync(fileMock.Object);

            // Assert - Operación todo-o-nada: no se creó ningún jugador
            Assert.False(result.Exito);
            Assert.Equal(2, result.TotalProcesados);
            Assert.Equal(0, result.TotalExitosos);
            Assert.True(result.TotalErrores > 0);
            
            // Verificar que NO se llamó AddAsync (ningún jugador se creó)
            _jugadorRepoMock.Verify(r => r.AddAsync(It.IsAny<Jugador>()), Times.Never);
        }

        [Fact]
        public async Task ProcessBatchFileAsync_WhenUnexpectedError_ThrowsBatchProcessingException()
        {
            // Arrange
            var batchRequest = new JugadorBatchRequestDto
            {
                Jugadores = new List<JugadorBatchItemDto>
                {
                    new JugadorBatchItemDto
                    {
                        Id = 1,
                        Nombre = "Patrick Mahomes",
                        Posicion = "QB",
                        EquipoNFLId = 1
                    }
                }
            };
            var jsonContent = JsonSerializer.Serialize(batchRequest);
            var fileContent = Encoding.UTF8.GetBytes(jsonContent);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.json");
            fileMock.Setup(f => f.Length).Returns(fileContent.Length);

            _batchFileServiceMock.Setup(s => s.ReadFileContentAsync(It.IsAny<IFormFile>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BatchProcessingException>(async () =>
                await _jugadorService.ProcessBatchFileAsync(fileMock.Object));

            Assert.Contains("Error interno del servidor", exception.Message);
        }

        #endregion
    }
}

