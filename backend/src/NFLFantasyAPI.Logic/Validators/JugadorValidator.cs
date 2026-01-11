using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Logic.Validators
{
    /// <summary>
    /// Validador centralizado para todas las validaciones relacionadas con jugadores
    /// REFACTORIZADO SPRINT 3: Modularizado con métodos pequeños y claros, agrupados por responsabilidad
    /// </summary>
    public class JugadorValidator
    {
        private readonly IJugadorRepository _jugadorRepository;
        private readonly IEquipoNFLRepository _equipoNFLRepository;

        #region Constantes y Constantes Estáticas

        // Posiciones válidas de la NFL
        private static readonly HashSet<string> PosicionesValidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "QB", "RB", "WR", "TE", "K", "DEF", "OL", "DL", "LB", "DB", "FB", "P", "LS"
        };

        // Estados válidos para un jugador
        private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "Activo", "Inactivo"
        };

        #endregion

        #region Constructor

        public JugadorValidator(
            IJugadorRepository jugadorRepository,
            IEquipoNFLRepository equipoNFLRepository)
        {
            _jugadorRepository = jugadorRepository;
            _equipoNFLRepository = equipoNFLRepository;
        }

        #endregion

        #region Validaciones de Datos Básicos

        /// <summary>
        /// Valida los campos requeridos para crear un jugador
        /// MÓDULO: Validaciones de datos básicos
        /// </summary>
        public void ValidarCamposRequeridos(string nombre, string posicion, int equipoNFLId)
        {
            ValidarNombreRequerido(nombre);
            ValidarPosicionRequerida(posicion);
            ValidarEquipoNFLIdRequerido(equipoNFLId);
        }

        /// <summary>
        /// Valida que el nombre no esté vacío
        /// </summary>
        public void ValidarNombreRequerido(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidationException("Nombre", "El nombre es requerido");
        }

        /// <summary>
        /// Valida que la posición no esté vacía
        /// </summary>
        public void ValidarPosicionRequerida(string posicion)
        {
            if (string.IsNullOrWhiteSpace(posicion))
                throw new ValidationException("Posicion", "La posición es requerida");
        }

        /// <summary>
        /// Valida que el ID del equipo NFL sea válido
        /// </summary>
        public void ValidarEquipoNFLIdRequerido(int equipoNFLId)
        {
            if (equipoNFLId <= 0)
                throw new ValidationException("EquipoNFLId", "El ID del equipo NFL debe ser mayor a 0");
        }

        /// <summary>
        /// Valida que la posición sea válida según las posiciones de la NFL
        /// </summary>
        public void ValidarPosicionValida(string posicion)
        {
            if (string.IsNullOrWhiteSpace(posicion))
                return;

            if (!PosicionesValidas.Contains(posicion.Trim()))
            {
                throw new ValidationException("Posicion",
                    $"La posición '{posicion}' no es válida. Posiciones válidas: {string.Join(", ", PosicionesValidas)}");
            }
        }

        #endregion

        #region Validaciones de Estado del Jugador

        /// <summary>
        /// Valida que el estado del jugador sea válido
        /// MÓDULO: Validaciones de estado
        /// </summary>
        public void ValidarEstadoValido(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return; // Estado es opcional en actualización

            if (!EstadosValidos.Contains(estado.Trim()))
            {
                throw new ValidationException("Estado",
                    $"El estado '{estado}' no es válido. Estados válidos: {string.Join(", ", EstadosValidos)}");
            }
        }

        /// <summary>
        /// Valida que el jugador esté activo para ciertas operaciones
        /// </summary>
        public void ValidarJugadorActivo(Jugador jugador)
        {
            if (jugador == null)
                throw new JugadorNotFoundException(0);

            if (!string.Equals(jugador.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("Estado",
                    $"El jugador '{jugador.Nombre}' no está activo. Estado actual: {jugador.Estado}");
            }
        }

        #endregion

        #region Validaciones de Relaciones (Equipo NFL, Duplicados)

        /// <summary>
        /// Valida que el equipo NFL existe
        /// </summary>
        public async Task ValidarEquipoExisteAsync(int equipoNFLId)
        {
            if (!await _jugadorRepository.EquipoExistsAsync(equipoNFLId))
                throw new EquipoNFLNotFoundException(equipoNFLId);
        }

        /// <summary>
        /// Valida que no exista un jugador duplicado (mismo nombre en el mismo equipo)
        /// </summary>
        public async Task ValidarNoDuplicadoAsync(string nombre, int equipoNFLId, int? jugadorIdActual = null)
        {
            var existe = await _jugadorRepository.ExistsInEquipoAsync(nombre, equipoNFLId);

            if (existe)
            {
                // Si es una actualización y es el mismo jugador, no hay problema
                if (jugadorIdActual.HasValue)
                {
                    var jugadorExistente = (await _jugadorRepository.GetByEquipoAsync(equipoNFLId))
                        .FirstOrDefault(j => string.Equals(j.Nombre.Trim(), nombre.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (jugadorExistente != null && jugadorExistente.Id == jugadorIdActual.Value)
                        return; // Es el mismo jugador, permitir la actualización
                }

                throw new JugadorDuplicadoException(nombre, equipoNFLId);
            }
        }

        #endregion

        #region Validaciones de URLs y Archivos

        /// <summary>
        /// Valida que las URLs de imágenes tengan formato válido
        /// MÓDULO: Validaciones de URLs y archivos
        /// </summary>
        public void ValidarUrlsValidas(string? imagenUrl, string? thumbnailUrl = null)
        {
            ValidarUrlValida(imagenUrl, "ImagenUrl");
            ValidarUrlValida(thumbnailUrl, "ThumbnailUrl");
        }

        /// <summary>
        /// Valida que una URL tenga formato válido (HTTP o HTTPS)
        /// </summary>
        private void ValidarUrlValida(string? url, string campoNombre)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException(campoNombre,
                    $"La URL de {campoNombre} '{url}' no tiene un formato válido");
            }
        }

        /// <summary>
        /// Valida que un archivo JSON sea válido para batch
        /// </summary>
        public void ValidarArchivoJson(string? fileName, long fileLength)
        {
            ValidarNombreArchivoRequerido(fileName);
            ValidarArchivoNoVacio(fileLength);
            ValidarExtensionJson(fileName);
        }

        /// <summary>
        /// Valida que el nombre del archivo no esté vacío
        /// </summary>
        private void ValidarNombreArchivoRequerido(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new InvalidFileException("El nombre del archivo es requerido");
        }

        /// <summary>
        /// Valida que el archivo no esté vacío
        /// </summary>
        private void ValidarArchivoNoVacio(long fileLength)
        {
            if (fileLength == 0)
                throw new InvalidFileException("El archivo está vacío");
        }

        /// <summary>
        /// Valida que el archivo tenga extensión JSON
        /// </summary>
        private void ValidarExtensionJson(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var extension = Path.GetExtension(fileName).ToLower();
            if (extension != ".json")
                throw new InvalidFileException(fileName, $"El archivo debe ser de tipo JSON (.json). Recibido: {extension}");
        }

        /// <summary>
        /// Valida un archivo de imagen
        /// </summary>
        public void ValidarArchivoImagen(string? contentType, long fileLength, long maxSizeMB = 5)
        {
            ValidarArchivoNoVacio(fileLength);
            ValidarTipoImagen(contentType);
            ValidarTamañoArchivo(fileLength, maxSizeMB);
        }

        /// <summary>
        /// Valida que el archivo sea de un tipo de imagen permitido
        /// </summary>
        private void ValidarTipoImagen(string? contentType)
        {
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            if (string.IsNullOrWhiteSpace(contentType) || !allowedTypes.Contains(contentType.ToLower()))
                throw new InvalidFileException("Solo se permiten imágenes JPEG o PNG");
        }

        /// <summary>
        /// Valida que el tamaño del archivo no exceda el límite
        /// </summary>
        private void ValidarTamañoArchivo(long fileLength, long maxSizeMB)
        {
            var maxSizeBytes = maxSizeMB * 1024 * 1024;
            if (fileLength > maxSizeBytes)
                throw new InvalidFileException($"El tamaño máximo permitido es {maxSizeMB} MB");
        }

        #endregion

        #region Métodos Consolidados de Validación

        /// <summary>
        /// Valida todos los datos básicos para crear un jugador (método consolidado)
        /// MÓDULO: Método consolidado que agrupa validaciones básicas
        /// </summary>
        public async Task ValidarParaCrearAsync(CrearJugadorDto dto)
        {
            // Validaciones de datos básicos
            ValidarCamposRequeridos(dto.Nombre, dto.Posicion, dto.EquipoNFLId);
            ValidarPosicionValida(dto.Posicion);
            
            // Validaciones de relaciones
            await ValidarEquipoExisteAsync(dto.EquipoNFLId);
            await ValidarNoDuplicadoAsync(dto.Nombre, dto.EquipoNFLId);
            
            // Validaciones de URLs
            ValidarUrlsValidas(dto.ImagenUrl, dto.ThumbnailUrl);
        }

        /// <summary>
        /// Valida todos los datos para actualizar un jugador (método consolidado)
        /// MÓDULO: Método consolidado que agrupa validaciones de actualización
        /// </summary>
        public async Task ValidarParaActualizarAsync(ActualizarJugadorDto dto, Jugador jugadorExistente)
        {
            // Validar equipo si se proporciona
            if (dto.EquipoNFLId.HasValue)
                await ValidarEquipoExisteAsync(dto.EquipoNFLId.Value);

            // Validar posición si se proporciona
            if (!string.IsNullOrWhiteSpace(dto.Posicion))
                ValidarPosicionValida(dto.Posicion);

            // Validar estado si se proporciona
            if (!string.IsNullOrWhiteSpace(dto.Estado))
                ValidarEstadoValido(dto.Estado);

            // Validar duplicados si se cambia el nombre
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
            {
                var equipoId = dto.EquipoNFLId ?? jugadorExistente.EquipoNFLId;
                await ValidarNoDuplicadoAsync(dto.Nombre, equipoId, jugadorExistente.Id);
            }

            // Validar URLs si se proporcionan
            ValidarUrlsValidas(dto.ImagenUrl, dto.ThumbnailUrl);
        }

        #endregion

        #region Validaciones de Batch

        /// <summary>
        /// Valida TODOS los jugadores de un batch antes de crear cualquiera
        /// REUTILIZA los métodos de validación individuales para evitar duplicación
        /// </summary>
        public async Task<List<BatchValidationError>> ValidarBatchAsync(List<JugadorBatchItemDto> jugadores)
        {
            var errors = new List<BatchValidationError>();

            // Validar IDs duplicados dentro del batch
            errors.AddRange(ValidarIdsDuplicadosEnBatch(jugadores));

            // Obtener datos necesarios en batch para optimizar consultas
            var equipoIds = jugadores.Select(j => j.EquipoNFLId).Distinct().ToList();
            var equiposExistentes = await _equipoNFLRepository.GetAllAsync();
            var equiposExistentesIds = equiposExistentes
                .Where(e => equipoIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToHashSet();

            // Obtener jugadores existentes en los equipos relevantes (para validar duplicados)
            var jugadoresExistentes = new List<Jugador>();
            foreach (var equipoId in equipoIds)
            {
                var jugadoresEquipo = await _jugadorRepository.GetByEquipoAsync(equipoId);
                jugadoresExistentes.AddRange(jugadoresEquipo);
            }

            // Validar cada jugador del batch REUTILIZANDO los métodos de validación individuales
            for (int i = 0; i < jugadores.Count; i++)
            {
                var jugador = jugadores[i];
                var playerName = jugador.Nombre ?? "Sin nombre";

                // Validar ID positivo (validación específica de batch)
                if (jugador.Id <= 0)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = playerName,
                        ErrorMessage = $"El ID debe ser un número positivo mayor a 0 (valor actual: {jugador.Id})",
                        ErrorType = "validation"
                    });
                    continue;
                }

                // REUTILIZAR: Validar campos requeridos usando el método existente
                try
                {
                    ValidarCamposRequeridos(jugador.Nombre ?? "", jugador.Posicion ?? "", jugador.EquipoNFLId);
                }
                catch (ValidationException ex)
                {
                    // Capturar errores de validación y agregarlos a la lista
                    foreach (var error in ex.Errores)
                    {
                        errors.Add(new BatchValidationError
                        {
                            PlayerId = jugador.Id,
                            PlayerName = playerName,
                            ErrorMessage = error.Value,
                            ErrorType = "validation"
                        });
                    }
                    continue; // Si hay errores en campos requeridos, continuar con el siguiente
                }

                // REUTILIZAR: Validar posición válida usando el método existente
                try
                {
                    ValidarPosicionValida(jugador.Posicion);
                }
                catch (ValidationException ex)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = playerName,
                        ErrorMessage = ex.Message,
                        ErrorType = "validation"
                    });
                    continue;
                }

                // REUTILIZAR: Validar URLs usando el método existente
                try
                {
                    ValidarUrlsValidas(jugador.ImagenUrl);
                }
                catch (ValidationException ex)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = playerName,
                        ErrorMessage = ex.Message,
                        ErrorType = "validation"
                    });
                    continue;
                }

                // REUTILIZAR: Validar que el equipo NFL existe usando el método existente
                if (!equiposExistentesIds.Contains(jugador.EquipoNFLId))
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = playerName,
                        ErrorMessage = $"El equipo NFL con ID {jugador.EquipoNFLId} no existe",
                        ErrorType = "not_found"
                    });
                    continue;
                }

                // Validar duplicados en la base de datos (optimizado para batch - ya tenemos los jugadores existentes cargados)
                var existeDuplicadoEnBD = jugadoresExistentes.Any(j =>
                    string.Equals(j.Nombre.Trim(), jugador.Nombre.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    j.EquipoNFLId == jugador.EquipoNFLId);

                if (existeDuplicadoEnBD)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = playerName,
                        ErrorMessage = $"Ya existe un jugador con el nombre '{jugador.Nombre}' en el equipo NFL especificado",
                        ErrorType = "duplicate"
                    });
                    continue;
                }

                // Validar duplicados dentro del mismo batch (nombre + equipo) - validación específica de batch
                var duplicadoEnBatch = jugadores
                    .Where((j, idx) => idx != i) // Excluir el jugador actual
                    .Any(j => string.Equals(j.Nombre?.Trim(), jugador.Nombre?.Trim(), StringComparison.OrdinalIgnoreCase)
                                && j.EquipoNFLId == jugador.EquipoNFLId);

                if (duplicadoEnBatch)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = playerName,
                        ErrorMessage = $"El jugador '{jugador.Nombre}' aparece duplicado en el archivo para el mismo equipo",
                        ErrorType = "duplicate"
                    });
                }
            }

            return errors;
        }

        /// <summary>
        /// Valida que no haya IDs duplicados dentro del batch
        /// </summary>
        private List<BatchValidationError> ValidarIdsDuplicadosEnBatch(List<JugadorBatchItemDto> jugadores)
        {
            var errors = new List<BatchValidationError>();
            var idsEnBatch = new Dictionary<int, int>(); // ID -> contador de apariciones

            foreach (var jugador in jugadores)
            {
                if (idsEnBatch.ContainsKey(jugador.Id))
                    idsEnBatch[jugador.Id]++;
                else
                    idsEnBatch[jugador.Id] = 1;
            }

            var idsDuplicados = idsEnBatch.Where(kvp => kvp.Value > 1).Select(kvp => kvp.Key).ToList();

            if (idsDuplicados.Any())
            {
                foreach (var id in idsDuplicados)
                {
                    var jugadoresConId = jugadores.Where(j => j.Id == id).ToList();
                    foreach (var jugador in jugadoresConId)
                    {
                        errors.Add(new BatchValidationError
                        {
                            PlayerId = jugador.Id,
                            PlayerName = jugador.Nombre,
                            ErrorMessage = $"El ID {id} aparece {idsEnBatch[id]} veces en el archivo. Cada ID debe ser único",
                            ErrorType = "duplicate"
                        });
                    }
                }
            }

            return errors;
        }

        #endregion
    }
}
