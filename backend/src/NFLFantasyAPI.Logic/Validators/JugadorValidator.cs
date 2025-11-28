using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Logic.Validators
{
    /// <summary>
    /// Validador centralizado para todas las validaciones relacionadas con jugadores
    /// </summary>
    public class JugadorValidator
    {
        private readonly IJugadorRepository _jugadorRepository;
        private readonly IEquipoNFLRepository _equipoNFLRepository;

        // Posiciones válidas de la NFL
        private static readonly HashSet<string> PosicionesValidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "QB", "RB", "WR", "TE", "K", "DEF", "OL", "DL", "LB", "DB", "FB", "P", "LS"
        };

        public JugadorValidator(
            IJugadorRepository jugadorRepository,
            IEquipoNFLRepository equipoNFLRepository)
        {
            _jugadorRepository = jugadorRepository;
            _equipoNFLRepository = equipoNFLRepository;
        }

        /// <summary>
        /// Valida los campos requeridos para crear un jugador
        /// </summary>
        public void ValidarCamposRequeridos(string nombre, string posicion, int equipoNFLId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidationException("Nombre", "El nombre es requerido");

            if (string.IsNullOrWhiteSpace(posicion))
                throw new ValidationException("Posicion", "La posición es requerida");

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

        /// <summary>
        /// Valida que las URLs de imágenes tengan formato válido
        /// </summary>
        public void ValidarUrlsValidas(string? imagenUrl, string? thumbnailUrl = null)
        {
            if (!string.IsNullOrWhiteSpace(imagenUrl))
            {
                if (!Uri.TryCreate(imagenUrl, UriKind.Absolute, out var uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    throw new ValidationException("ImagenUrl",
                        $"La URL de imagen '{imagenUrl}' no tiene un formato válido");
                }
            }

            if (!string.IsNullOrWhiteSpace(thumbnailUrl))
            {
                if (!Uri.TryCreate(thumbnailUrl, UriKind.Absolute, out var uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    throw new ValidationException("ThumbnailUrl",
                        $"La URL de thumbnail '{thumbnailUrl}' no tiene un formato válido");
                }
            }
        }

        /// <summary>
        /// Valida que un archivo JSON sea válido para batch
        /// </summary>
        public void ValidarArchivoJson(string? fileName, long fileLength)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new InvalidFileException("El nombre del archivo es requerido");

            if (fileLength == 0)
                throw new InvalidFileException("El archivo está vacío");

            var extension = Path.GetExtension(fileName).ToLower();
            if (extension != ".json")
                throw new InvalidFileException(fileName, $"El archivo debe ser de tipo JSON (.json). Recibido: {extension}");
        }

        /// <summary>
        /// Valida un archivo de imagen
        /// </summary>
        public void ValidarArchivoImagen(string? contentType, long fileLength, long maxSizeMB = 5)
        {
            if (fileLength == 0)
                throw new InvalidFileException("El archivo está vacío");

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            if (string.IsNullOrWhiteSpace(contentType) || !allowedTypes.Contains(contentType.ToLower()))
                throw new InvalidFileException("Solo se permiten imágenes JPEG o PNG");

            var maxSizeBytes = maxSizeMB * 1024 * 1024;
            if (fileLength > maxSizeBytes)
                throw new InvalidFileException($"El tamaño máximo permitido es {maxSizeMB} MB");
        }

        /// <summary>
        /// Valida TODOS los jugadores de un batch antes de crear cualquiera
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

            // Obtener jugadores existentes en los equipos relevantes
            var jugadoresExistentes = new List<Jugador>();
            foreach (var equipoId in equipoIds)
            {
                var jugadoresEquipo = await _jugadorRepository.GetByEquipoAsync(equipoId);
                jugadoresExistentes.AddRange(jugadoresEquipo);
            }

            // Validar cada jugador del batch
            for (int i = 0; i < jugadores.Count; i++)
            {
                var jugador = jugadores[i];

                // Validar ID positivo
                if (jugador.Id <= 0)
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = jugador.Nombre ?? "Sin nombre",
                        ErrorMessage = $"El ID debe ser un número positivo mayor a 0 (valor actual: {jugador.Id})",
                        ErrorType = "validation"
                    });
                    continue;
                }

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

                // Validar posición válida
                if (!PosicionesValidas.Contains(jugador.Posicion.Trim()))
                {
                    errors.Add(new BatchValidationError
                    {
                        PlayerId = jugador.Id,
                        PlayerName = jugador.Nombre,
                        ErrorMessage = $"La posición '{jugador.Posicion}' no es válida. Posiciones válidas: {string.Join(", ", PosicionesValidas)}",
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

                // Validar URL de imagen si está presente
                if (!string.IsNullOrWhiteSpace(jugador.ImagenUrl))
                {
                    if (!Uri.TryCreate(jugador.ImagenUrl, UriKind.Absolute, out var uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        errors.Add(new BatchValidationError
                        {
                            PlayerId = jugador.Id,
                            PlayerName = jugador.Nombre,
                            ErrorMessage = $"La URL de imagen '{jugador.ImagenUrl}' no tiene un formato válido",
                            ErrorType = "validation"
                        });
                        continue;
                    }
                }

                // Validar que el equipo NFL existe
                if (!equiposExistentesIds.Contains(jugador.EquipoNFLId))
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

                // Validar duplicados en la base de datos
                var existeDuplicado = jugadoresExistentes.Any(j =>
                    string.Equals(j.Nombre.Trim(), jugador.Nombre.Trim(), StringComparison.OrdinalIgnoreCase) &&
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

                // Validar duplicados dentro del mismo batch (nombre + equipo)
                var duplicadoEnBatch = jugadores
                    .Where((j, idx) => idx != i) // Excluir el jugador actual
                    .Any(j => string.Equals(j.Nombre.Trim(), jugador.Nombre.Trim(), StringComparison.OrdinalIgnoreCase)
                                && j.EquipoNFLId == jugador.EquipoNFLId);

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
    }
}
