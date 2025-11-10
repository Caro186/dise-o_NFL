using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.DTOs;
using NFLFantasyAPI.Models;
using System.Text.Json;

namespace NFLFantasyAPI.Services
{
    /// <summary>
    /// Servicio para procesar lotes (batch) de jugadores NFL desde archivos JSON
    /// Implementa la lógica "todo-o-nada": si hay al menos un error, no se crea ningún jugador
    /// </summary>
    public class JugadorBatchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<JugadorBatchService> _logger;

        public JugadorBatchService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<JugadorBatchService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Procesa un archivo JSON con múltiples jugadores
        /// </summary>
        /// <param name="file">Archivo JSON con los datos de los jugadores</param>
        /// <returns>Resultado del procesamiento con reporte de éxitos y errores</returns>
        public async Task<JugadorBatchResultDto> ProcessBatchFileAsync(IFormFile file)
        {
            var result = new JugadorBatchResultDto
            {
                Exito = false,
                TotalProcesados = 0,
                TotalExitosos = 0,
                TotalErrores = 0
            };

            try
            {
                // 1. Validar que el archivo no esté vacío
                if (file == null || file.Length == 0)
                {
                    result.Mensaje = "El archivo está vacío o no es válido";
                    result.Errores.Add(new JugadorBatchErrorDto
                    {
                        Error = "Archivo vacío o no válido"
                    });
                    await MoveFileToProcessedFolderAsync(file?.FileName ?? "unknown.json", false);
                    return result;
                }

                // 2. Leer y parsear el archivo JSON
                JugadorBatchRequestDto? batchRequest;
                try
                {
                    using var stream = file.OpenReadStream();
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
                    await MoveFileToProcessedFolderAsync(file.FileName, false);
                    return result;
                }

                if (batchRequest == null || batchRequest.Jugadores == null || !batchRequest.Jugadores.Any())
                {
                    result.Mensaje = "El archivo JSON no contiene jugadores válidos";
                    result.Errores.Add(new JugadorBatchErrorDto
                    {
                        Error = "No se encontraron jugadores en el archivo"
                    });
                    await MoveFileToProcessedFolderAsync(file.FileName, false);
                    return result;
                }

                result.TotalProcesados = batchRequest.Jugadores.Count;

                // 3. Validar TODOS los jugadores antes de crear cualquiera
                var validationErrors = await ValidateAllPlayersAsync(batchRequest.Jugadores);

                if (validationErrors.Any())
                {
                    // Si hay errores, NO crear ningún jugador (todo-o-nada)
                    result.Mensaje = $"Se encontraron {validationErrors.Count} errores. No se creó ningún jugador (operación todo-o-nada)";
                    result.TotalErrores = validationErrors.Count;
                    result.Errores = validationErrors.Select(e => new JugadorBatchErrorDto
                    {
                        Id = e.PlayerId,
                        Nombre = e.PlayerName,
                        Error = e.ErrorMessage
                    }).ToList();
                    
                    await MoveFileToProcessedFolderAsync(file.FileName, false);
                    result.ArchivoMovidoA = GetProcessedFileName(file.FileName, false);
                    return result;
                }

                // 4. Si NO hay errores, crear TODOS los jugadores en una transacción
                var createdPlayers = await CreateAllPlayersInTransactionAsync(batchRequest.Jugadores);

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

                    await MoveFileToProcessedFolderAsync(file.FileName, true);
                    result.ArchivoMovidoA = GetProcessedFileName(file.FileName, true);
                }
                else
                {
                    result.Mensaje = "No se pudieron crear los jugadores";
                    result.Errores.Add(new JugadorBatchErrorDto
                    {
                        Error = "Error desconocido al crear jugadores"
                    });
                    await MoveFileToProcessedFolderAsync(file.FileName, false);
                    result.ArchivoMovidoA = GetProcessedFileName(file.FileName, false);
                }

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
                
                if (file != null)
                {
                    await MoveFileToProcessedFolderAsync(file.FileName, false);
                    result.ArchivoMovidoA = GetProcessedFileName(file.FileName, false);
                }
                
                return result;
            }
        }

        /// <summary>
        /// Valida todos los jugadores del batch ANTES de crear cualquiera
        /// </summary>
        private async Task<List<BatchValidationError>> ValidateAllPlayersAsync(List<JugadorBatchItemDto> jugadores)
        {
            var errors = new List<BatchValidationError>();

            // Obtener todos los IDs de equipos NFL para validar en una sola consulta
            var equipoIds = jugadores.Select(j => j.EquipoNFLId).Distinct().ToList();
            var equiposExistentes = await _context.EquiposNFL
                .Where(e => equipoIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync();

            // Obtener todos los jugadores existentes para validar duplicados
            var nombresYEquipos = jugadores.Select(j => new { j.Nombre, j.EquipoNFLId }).ToList();
            var jugadoresExistentes = await _context.Jugadores
                .Where(j => equipoIds.Contains(j.EquipoNFLId))
                .Select(j => new { j.Nombre, j.EquipoNFLId })
                .ToListAsync();

            foreach (var jugador in jugadores)
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(jugador.Nombre))
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = "Sin nombre",
                        ErrorMessage = "El nombre es requerido",
                        ErrorType = "validation"
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(jugador.Posicion))
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = jugador.Nombre,
                        ErrorMessage = "La posición es requerida",
                        ErrorType = "validation"
                    });
                    continue;
                }

                if (jugador.EquipoNFLId <= 0)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = jugador.Nombre,
                        ErrorMessage = "El ID del equipo NFL debe ser mayor a 0",
                        ErrorType = "validation"
                    });
                    continue;
                }

                // Validar que el equipo NFL existe
                if (!equiposExistentes.Contains(jugador.EquipoNFLId))
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = jugador.Nombre,
                        ErrorMessage = $"El equipo NFL con ID {jugador.EquipoNFLId} no existe",
                        ErrorType = "not_found"
                    });
                    continue;
                }

                // Validar que no existe un jugador con el mismo nombre en el mismo equipo
                var existeDuplicado = jugadoresExistentes.Any(j =>
                    j.Nombre.Trim().ToLower() == jugador.Nombre.Trim().ToLower() &&
                    j.EquipoNFLId == jugador.EquipoNFLId);

                if (existeDuplicado)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = jugador.Nombre,
                        ErrorMessage = $"Ya existe un jugador con el nombre '{jugador.Nombre}' en el equipo NFL especificado",
                        ErrorType = "duplicate"
                    });
                    continue;
                }

                // Validar duplicados dentro del mismo batch
                var duplicadoEnBatch = jugadores
                    .Where(j => j.Nombre.Trim().ToLower() == jugador.Nombre.Trim().ToLower() &&
                               j.EquipoNFLId == jugador.EquipoNFLId)
                    .Count() > 1;

                if (duplicadoEnBatch)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = jugador.Nombre,
                        ErrorMessage = $"El jugador '{jugador.Nombre}' aparece duplicado en el archivo para el mismo equipo",
                        ErrorType = "duplicate"
                    });
                }
            }

            return errors;
        }

        /// <summary>
        /// Crea todos los jugadores en una única transacción (todo-o-nada)
        /// </summary>
        private async Task<List<Jugador>> CreateAllPlayersInTransactionAsync(List<JugadorBatchItemDto> jugadores)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var createdPlayers = new List<Jugador>();

                foreach (var jugadorDto in jugadores)
                {
                    var jugador = new Jugador
                    {
                        Nombre = jugadorDto.Nombre.Trim(),
                        Posicion = jugadorDto.Posicion.Trim(),
                        EquipoNFLId = jugadorDto.EquipoNFLId,
                        ImagenUrl = jugadorDto.ImagenUrl?.Trim(),
                        ThumbnailUrl = jugadorDto.ImagenUrl?.Trim(), // Se autogenera del ImagenUrl
                        Estado = "Activo",
                        FechaCreacion = DateTime.UtcNow
                    };

                    _context.Jugadores.Add(jugador);
                    createdPlayers.Add(jugador);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Cargar los equipos NFL para el reporte
                foreach (var jugador in createdPlayers)
                {
                    await _context.Entry(jugador)
                        .Reference(j => j.EquipoNFL)
                        .LoadAsync();
                }

                _logger.LogInformation($"Se crearon exitosamente {createdPlayers.Count} jugadores en batch");

                return createdPlayers;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear jugadores en transacción, realizando rollback");
                throw;
            }
        }

        /// <summary>
        /// Mueve el archivo procesado a la carpeta correspondiente con el formato requerido
        /// Formato: {Resultado}_{FechaHora}_{NombreOriginal}.json
        /// </summary>
        private async Task<string> MoveFileToProcessedFolderAsync(string originalFileName, bool success)
        {
            try
            {
                // Crear carpeta de archivos procesados si no existe
                var processedFolder = Path.Combine(_environment.WebRootPath, "processed", "jugadores");
                if (!Directory.Exists(processedFolder))
                {
                    Directory.CreateDirectory(processedFolder);
                }

                // Generar nombre del archivo procesado
                var resultado = success ? "Exito" : "Fallo";
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
                var extension = Path.GetExtension(originalFileName);
                var newFileName = $"{resultado}_{timestamp}_{fileNameWithoutExtension}{extension}";
                var newFilePath = Path.Combine(processedFolder, newFileName);

                // Nota: En un escenario real, aquí se movería el archivo físico
                // Como estamos procesando desde un stream, solo generamos el nombre
                _logger.LogInformation($"Archivo procesado: {newFileName}");

                return newFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mover archivo a carpeta de procesados");
                return originalFileName;
            }
        }

        /// <summary>
        /// Genera el nombre del archivo procesado
        /// </summary>
        private string GetProcessedFileName(string originalFileName, bool success)
        {
            var resultado = success ? "Exito" : "Fallo";
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName);
            return $"{resultado}_{timestamp}_{fileNameWithoutExtension}{extension}";
        }
    }
}