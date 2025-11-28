using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Logic.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.CrossCutting.Configuration;

namespace NFLFantasyAPI.Logic.Services
{
    public class JugadorService : IJugadorService
    {
        private readonly IJugadorRepository _jugadorRepository;
        private readonly IEquipoNFLRepository _equipoNFLRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<JugadorService> _logger;
        private readonly FileServerSettings _fileServerSettings;
        private readonly JugadorValidator _validator;
        private readonly IBatchFileProcessingService _batchFileService;

        public JugadorService(
            IJugadorRepository jugadorRepository,
            IEquipoNFLRepository equipoNFLRepository,
            IWebHostEnvironment environment,
            ILogger<JugadorService> logger,
            IOptions<FileServerSettings> fileServerSettings,
            JugadorValidator validator,
            IBatchFileProcessingService batchFileService)
        {
            _jugadorRepository = jugadorRepository;
            _equipoNFLRepository = equipoNFLRepository;
            _environment = environment;
            _logger = logger;
            _fileServerSettings = fileServerSettings.Value;
            _validator = validator;
            _batchFileService = batchFileService;
        }

        public async Task<ServiceResult> GetAllAsync()
        {
            var jugadores = await _jugadorRepository.GetAllAsync();

            var dto = jugadores.Select(j => new JugadorListDto
            {
                Id = j.Id,
                Nombre = j.Nombre,
                Posicion = j.Posicion,
                NombreEquipoNFL = j.EquipoNFL.Nombre,
                ThumbnailUrl = j.ThumbnailUrl,
                Estado = j.Estado
            }).ToList();

            return ServiceResult.Ok(dto);
        }

        public async Task<ServiceResult> GetByIdAsync(int id)
        {
            try
            {
                var jugador = await _jugadorRepository.GetByIdAsync(id);
                if (jugador == null)
                    throw new JugadorNotFoundException(id);

                var dto = new JugadorResponseDto
                {
                    Id = jugador.Id,
                    Nombre = jugador.Nombre,
                    Posicion = jugador.Posicion,
                    EquipoNFLId = jugador.EquipoNFLId,
                    NombreEquipoNFL = jugador.EquipoNFL.Nombre,
                    CiudadEquipoNFL = jugador.EquipoNFL.Ciudad,
                    ImagenUrl = jugador.ImagenUrl,
                    ThumbnailUrl = jugador.ThumbnailUrl,
                    Estado = jugador.Estado,
                    FechaCreacion = jugador.FechaCreacion,
                    FechaActualizacion = jugador.FechaActualizacion
                };

                return ServiceResult.Ok(dto);
            }
            catch (JugadorNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
        }

        public async Task<ServiceResult> CreateAsync(CrearJugadorDto dto)
        {
            try
            {
                // Validaciones usando el validador centralizado
                _validator.ValidarCamposRequeridos(dto.Nombre, dto.Posicion, dto.EquipoNFLId);
                _validator.ValidarPosicionValida(dto.Posicion);
                await _validator.ValidarEquipoExisteAsync(dto.EquipoNFLId);
                await _validator.ValidarNoDuplicadoAsync(dto.Nombre, dto.EquipoNFLId);
                _validator.ValidarUrlsValidas(dto.ImagenUrl, dto.ThumbnailUrl);

                // Crear el jugador usando el método interno (reutilizable)
                var jugador = await CrearJugadorInternoAsync(
                    dto.Nombre,
                    dto.Posicion,
                    dto.EquipoNFLId,
                    dto.ImagenUrl,
                    dto.ThumbnailUrl);

                return ServiceResult.Ok(new { mensaje = "Jugador creado correctamente", jugador.Id });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación al crear jugador: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (EquipoNFLNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (JugadorDuplicadoException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear jugador");
                return ServiceResult.Error("Error interno del servidor al crear el jugador");
            }
        }

        public async Task<ServiceResult> UpdateAsync(int id, ActualizarJugadorDto dto)
        {
            try
            {
                var jugador = await _jugadorRepository.GetByIdAsync(id);
                if (jugador == null)
                    throw new JugadorNotFoundException(id);

                // Validar equipo si se proporciona
                if (dto.EquipoNFLId.HasValue)
                    await _validator.ValidarEquipoExisteAsync(dto.EquipoNFLId.Value);

                // Validar posición si se proporciona
                if (!string.IsNullOrWhiteSpace(dto.Posicion))
                    _validator.ValidarPosicionValida(dto.Posicion);

                // Validar duplicados si se cambia el nombre
                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    var equipoId = dto.EquipoNFLId ?? jugador.EquipoNFLId;
                    await _validator.ValidarNoDuplicadoAsync(dto.Nombre, equipoId, id);
                }

                // Validar URLs si se proporcionan
                _validator.ValidarUrlsValidas(dto.ImagenUrl, dto.ThumbnailUrl);

                // Actualizar campos
                jugador.Nombre = dto.Nombre ?? jugador.Nombre;
                jugador.Posicion = dto.Posicion ?? jugador.Posicion;
                jugador.EquipoNFLId = dto.EquipoNFLId ?? jugador.EquipoNFLId;
                jugador.ImagenUrl = dto.ImagenUrl ?? jugador.ImagenUrl;
                jugador.ThumbnailUrl = dto.ThumbnailUrl ?? jugador.ThumbnailUrl;
                jugador.Estado = dto.Estado ?? jugador.Estado;
                jugador.FechaActualizacion = DateTime.UtcNow;

                await _jugadorRepository.UpdateAsync(jugador);

                return ServiceResult.Ok(new { mensaje = "Jugador actualizado correctamente" });
            }
            catch (JugadorNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación al actualizar jugador: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (EquipoNFLNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (JugadorDuplicadoException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar jugador {Id}", id);
                return ServiceResult.Error("Error interno del servidor al actualizar el jugador");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id, bool permanente)
        {
            try
            {
                var jugador = await _jugadorRepository.GetByIdAsync(id);
                if (jugador == null)
                    throw new JugadorNotFoundException(id);

                if (permanente)
                {
                    await _jugadorRepository.DeleteAsync(jugador);
                    return ServiceResult.Ok(new { mensaje = "Jugador eliminado permanentemente" });
                }

                jugador.Estado = "Inactivo";
                jugador.FechaActualizacion = DateTime.UtcNow;
                await _jugadorRepository.UpdateAsync(jugador);
                return ServiceResult.Ok(new { mensaje = "Jugador desactivado correctamente" });
            }
            catch (JugadorNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar jugador {Id}", id);
                return ServiceResult.Error("Error interno del servidor al eliminar el jugador");
            }
        }

        public async Task<ServiceResult> GetByEquipoAsync(int equipoId)
        {
            try
            {
                await _validator.ValidarEquipoExisteAsync(equipoId);

                var jugadores = await _jugadorRepository.GetByEquipoAsync(equipoId);

                var dto = jugadores.Select(j => new JugadorListDto
                {
                    Id = j.Id,
                    Nombre = j.Nombre,
                    Posicion = j.Posicion,
                    NombreEquipoNFL = j.EquipoNFL.Nombre,
                    ThumbnailUrl = j.ThumbnailUrl,
                    Estado = j.Estado
                }).ToList();

                return ServiceResult.Ok(dto);
            }
            catch (EquipoNFLNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
        }

        public async Task<ServiceResult> GetByPosicionAsync(string posicion)
        {
            var jugadores = await _jugadorRepository.GetByPosicionAsync(posicion);
            var dto = jugadores.Select(j => new JugadorListDto
            {
                Id = j.Id,
                Nombre = j.Nombre,
                Posicion = j.Posicion,
                NombreEquipoNFL = j.EquipoNFL.Nombre,
                ThumbnailUrl = j.ThumbnailUrl,
                Estado = j.Estado
            }).ToList();

            return ServiceResult.Ok(dto);
        }

        public async Task<ServiceResult> SubirImagenAsync(int id, IFormFile imagen)
        {
            try
            {
                // Validar que el jugador existe
                var jugador = await _jugadorRepository.GetByIdAsync(id);
                if (jugador == null)
                    throw new JugadorNotFoundException(id);

                // Validar el archivo usando el validador
                _validator.ValidarArchivoImagen(imagen.ContentType, imagen.Length, maxSizeMB: 5);

                // Crear directorio si no existe
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "jugadores");
                Directory.CreateDirectory(uploadsFolder);

                // Generar nombre único y guardar archivo
                var extension = Path.GetExtension(imagen.FileName);
                var fileName = $"{id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                // Actualizar la URL en la base de datos
                jugador.ImagenUrl = $"{_fileServerSettings.BaseUrl}/uploads/jugadores/{fileName}";
                await _jugadorRepository.SaveChangesAsync();

                _logger.LogInformation("Imagen subida exitosamente para el jugador {Id}", id);

                return ServiceResult.Ok(new
                {
                    mensaje = "Imagen subida exitosamente",
                    imagenUrl = jugador.ImagenUrl
                });
            }
            catch (JugadorNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (InvalidFileException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen del jugador {Id}", id);
                return ServiceResult.Error("Error interno del servidor al subir la imagen");
            }
        }

        public async Task<ServiceResult> SubirThumbnailAsync(int id, IFormFile thumbnail)
        {
            try
            {
                // Validar que el jugador existe
                var jugador = await _jugadorRepository.GetByIdAsync(id);
                if (jugador == null)
                    throw new JugadorNotFoundException(id);

                // Validar el archivo usando el validador
                _validator.ValidarArchivoImagen(thumbnail.ContentType, thumbnail.Length, maxSizeMB: 2);

                // Crear directorio si no existe
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "jugadores", "thumbnails");
                Directory.CreateDirectory(uploadsFolder);

                // Generar nombre único y guardar archivo
                var extension = Path.GetExtension(thumbnail.FileName);
                var fileName = $"{id}_thumb_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await thumbnail.CopyToAsync(stream);
                }

                // Actualizar la URL en la base de datos
                jugador.ThumbnailUrl = $"{_fileServerSettings.BaseUrl}/uploads/jugadores/thumbnails/{fileName}";
                await _jugadorRepository.SaveChangesAsync();

                _logger.LogInformation("Thumbnail subido exitosamente para el jugador {Id}", id);

                return ServiceResult.Ok(new
                {
                    mensaje = "Thumbnail subido exitosamente",
                    thumbnailUrl = jugador.ThumbnailUrl
                });
            }
            catch (JugadorNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (InvalidFileException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir thumbnail del jugador {Id}", id);
                return ServiceResult.Error("Error interno del servidor al subir el thumbnail");
            }
        }

        /// <summary>
        /// Procesa un archivo batch de jugadores (operación todo-o-nada)
        /// AHORA REUTILIZA LA CREACIÓN MANUAL
        /// </summary>
        public async Task<JugadorBatchResultDto> ProcessBatchFileAsync(IFormFile file)
        {
            var result = new JugadorBatchResultDto
            {
                Exito = false,
                TotalProcesados = 0,
                TotalExitosos = 0,
                TotalErrores = 0
            };

            byte[]? fileContent = null;

            try
            {
                // 1. Validar archivo usando el validador
                _validator.ValidarArchivoJson(file?.FileName, file?.Length ?? 0);

                // 2. Leer contenido del archivo usando el servicio de archivos
                fileContent = await _batchFileService.ReadFileContentAsync(file!);

                // 3. Parsear JSON
                JugadorBatchRequestDto? batchRequest;
                try
                {
                    using var stream = new MemoryStream(fileContent);
                    batchRequest = await JsonSerializer.DeserializeAsync<JugadorBatchRequestDto>(stream, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException ex)
                {
                    result.Mensaje = "Error al parsear el archivo JSON: formato inválido";
                    result.Errores.Add(new JugadorBatchErrorDto
                    {
                        Error = $"Formato JSON inválido: {ex.Message}"
                    });
                    var failedPath = await _batchFileService.SaveProcessedFileAsync(file.FileName, fileContent, false, "jugadores");
                    result.ArchivoMovidoA = failedPath;
                    return result;
                }

                if (batchRequest == null || batchRequest.Jugadores == null || !batchRequest.Jugadores.Any())
                {
                    result.Mensaje = "El archivo JSON no contiene jugadores válidos";
                    result.Errores.Add(new JugadorBatchErrorDto
                    {
                        Error = "No se encontraron jugadores en el archivo"
                    });
                    var failedPath = await _batchFileService.SaveProcessedFileAsync(file.FileName, fileContent, false, "jugadores");
                    result.ArchivoMovidoA = failedPath;
                    return result;
                }

                result.TotalProcesados = batchRequest.Jugadores.Count;

                // 4. Validar TODOS los jugadores usando el validador
                var validationErrors = await _validator.ValidarBatchAsync(batchRequest.Jugadores);

                if (validationErrors.Any())
                {
                    result.Mensaje = $"Se encontraron {validationErrors.Count} errores. No se creó ningún jugador (operación todo-o-nada)";
                    result.TotalErrores = validationErrors.Count;
                    result.Errores = validationErrors.Select(e => new JugadorBatchErrorDto
                    {
                        Id = e.PlayerId,
                        Nombre = e.PlayerName,
                        Error = e.ErrorMessage
                    }).ToList();

                    var failedPath = await _batchFileService.SaveProcessedFileAsync(file.FileName, fileContent, false, "jugadores");
                    result.ArchivoMovidoA = failedPath;
                    return result;
                }

                // 5. Crear TODOS los jugadores en transacción REUTILIZANDO el método de creación manual
                var createdPlayers = await CrearJugadoresEnTransaccionAsync(batchRequest.Jugadores);

                if (createdPlayers.Any())
                {
                    result.Exito = true;
                    result.TotalExitosos = createdPlayers.Count;
                    result.Mensaje = $"Se crearon exitosamente {createdPlayers.Count} jugadores";
                    result.JugadoresCreados = createdPlayers.Select(j => new JugadorCreatedDto
                    {
                        Id = j.Id,
                        Nombre = j.Nombre,
                        Posicion = j.Posicion,
                        NombreEquipoNFL = j.EquipoNFL?.Nombre ?? "N/A"
                    }).ToList();

                    var successPath = await _batchFileService.SaveProcessedFileAsync(file.FileName, fileContent, true, "jugadores");
                    result.ArchivoMovidoA = successPath;
                }
                else
                {
                    result.Mensaje = "No se pudieron crear los jugadores";
                    result.Errores.Add(new JugadorBatchErrorDto
                    {
                        Error = "Error desconocido al crear jugadores"
                    });
                    var failedPath = await _batchFileService.SaveProcessedFileAsync(file.FileName, fileContent, false, "jugadores");
                    result.ArchivoMovidoA = failedPath;
                }

                return result;
            }
            catch (InvalidFileException ex)
            {
                _logger.LogWarning($"Archivo inválido: {ex.Message}");
                result.Mensaje = ex.Message;
                result.Errores.Add(new JugadorBatchErrorDto { Error = ex.Message });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar el archivo batch de jugadores");
                result.Mensaje = $"Error inesperado: {ex.Message}";
                result.Errores.Add(new JugadorBatchErrorDto
                {
                    Error = $"Error del sistema: {ex.Message}"
                });

                if (file != null && fileContent != null)
                {
                    try
                    {
                        var failedPath = await _batchFileService.SaveProcessedFileAsync(file.FileName, fileContent, false, "jugadores");
                        result.ArchivoMovidoA = failedPath;
                    }
                    catch (Exception moveEx)
                    {
                        _logger.LogError(moveEx, "Error al mover archivo después de un fallo en el procesamiento");
                    }
                }

                return result;
            }
        }

        #region Métodos Privados Reutilizables

        /// <summary>
        /// Método interno reutilizable para crear un jugador
        /// Este método se usa tanto en la creación manual como en el batch
        /// </summary>
        private async Task<Jugador> CrearJugadorInternoAsync(
            string nombre,
            string posicion,
            int equipoNFLId,
            string? imagenUrl = null,
            string? thumbnailUrl = null)
        {
            var jugador = new Jugador
            {
                Nombre = nombre.Trim(),
                Posicion = posicion.Trim(),
                EquipoNFLId = equipoNFLId,
                ImagenUrl = imagenUrl?.Trim(),
                ThumbnailUrl = thumbnailUrl?.Trim(),
                Estado = "Activo",
                FechaCreacion = DateTime.UtcNow
            };

            await _jugadorRepository.AddAsync(jugador);
            return jugador;
        }

        /// <summary>
        /// Crea todos los jugadores en una única transacción (todo-o-nada)
        /// REUTILIZA el método de creación manual
        /// </summary>
        private async Task<List<Jugador>> CrearJugadoresEnTransaccionAsync(List<JugadorBatchItemDto> jugadores)
        {
            var createdPlayers = new List<Jugador>();

            try
            {
                using var transaction = await _jugadorRepository.BeginTransactionAsync();

                foreach (var jugadorDto in jugadores)
                {
                    // REUTILIZAR el método de creación manual
                    var jugador = await CrearJugadorInternoAsync(
                        jugadorDto.Nombre,
                        jugadorDto.Posicion,
                        jugadorDto.EquipoNFLId,
                        jugadorDto.ImagenUrl,
                        jugadorDto.ImagenUrl // Thumbnail se autogenera del ImagenUrl
                    );

                    createdPlayers.Add(jugador);
                }

                await _jugadorRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                // Cargar los equipos NFL para el reporte
                foreach (var jugador in createdPlayers)
                {
                    jugador.EquipoNFL = await _equipoNFLRepository.GetByIdAsync(jugador.EquipoNFLId);
                }

                _logger.LogInformation($"Se crearon exitosamente {createdPlayers.Count} jugadores en batch");

                return createdPlayers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear jugadores en transacción, realizando rollback");
                await _jugadorRepository.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion
    }
}
