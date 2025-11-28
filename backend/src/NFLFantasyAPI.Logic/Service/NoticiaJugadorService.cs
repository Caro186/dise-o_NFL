using Microsoft.Extensions.Logging;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Logic.Service
{
    public class NoticiaJugadorService : INoticiaJugadorService
    {
        private readonly INoticiaJugadorRepository _noticiaRepository;
        private readonly ILogger<NoticiaJugadorService> _logger;

        public NoticiaJugadorService(
            INoticiaJugadorRepository noticiaRepository,
            ILogger<NoticiaJugadorService> logger)
        {
            _noticiaRepository = noticiaRepository;
            _logger = logger;
        }

        public async Task<NoticiaJugadorResponseDto> CrearNoticiaAsync(CrearNoticiaJugadorDto dto, int autorId)
        {
            _logger.LogInformation("Creando noticia para jugador {JugadorId}", dto.JugadorId);

            // Validar que el jugador existe y está activo
            var jugadorExiste = await _noticiaRepository.ExisteJugadorAsync(dto.JugadorId);
            if (!jugadorExiste)
            {
                throw new InvalidOperationException("El jugador no existe o está inactivo");
            }

            // Validaciones específicas para noticias de lesión
            if (dto.EsLesion)
            {
                if (string.IsNullOrWhiteSpace(dto.ResumenLesion))
                {
                    throw new ArgumentException("El resumen de la lesión es obligatorio para noticias de lesión");
                }

                if (string.IsNullOrWhiteSpace(dto.DesignacionLesion))
                {
                    throw new ArgumentException("La designación de lesión es obligatoria para noticias de lesión");
                }

                // Validar que la designación sea válida
                var designacionesValidas = new[] { "O", "D", "Q", "P", "FP", "IR", "PUP", "SUS" };
                if (!designacionesValidas.Contains(dto.DesignacionLesion))
                {
                    throw new ArgumentException("Designación de lesión inválida. Valores permitidos: O, D, Q, P, FP, IR, PUP, SUS");
                }
            }
            else
            {
                // Si no es lesión, no debe tener campos de lesión
                dto.ResumenLesion = null;
                dto.DesignacionLesion = null;
            }

            // Crear la entidad de noticia
            var noticia = new NoticiaJugador
            {
                JugadorId = dto.JugadorId,
                Texto = dto.Texto.Trim(),
                EsLesion = dto.EsLesion,
                ResumenLesion = dto.ResumenLesion?.Trim(),
                DesignacionLesion = dto.DesignacionLesion,
                AutorId = autorId,
                FechaCreacion = DateTime.UtcNow,
                Estado = "Activa"
            };

            // Guardar la noticia
            var noticiaCreada = await _noticiaRepository.CrearAsync(noticia);

            // Si es noticia de lesión, actualizar la designación del jugador
            if (dto.EsLesion && !string.IsNullOrWhiteSpace(dto.DesignacionLesion))
            {
                await _noticiaRepository.ActualizarDesignacionJugadorAsync(dto.JugadorId, dto.DesignacionLesion);
                _logger.LogInformation(
                    "Designación del jugador {JugadorId} actualizada a {Designacion}",
                    dto.JugadorId,
                    dto.DesignacionLesion);
            }

            // Obtener la noticia completa con relaciones para la respuesta
            var noticiaCompleta = await _noticiaRepository.ObtenerPorIdAsync(noticiaCreada.Id);

            _logger.LogInformation(
                "Noticia {NoticiaId} creada exitosamente por usuario {AutorId} para jugador {JugadorId}",
                noticiaCreada.Id,
                autorId,
                dto.JugadorId);

            return MapearAResponseDto(noticiaCompleta!);
        }

        public async Task<List<NoticiaJugadorResponseDto>> ObtenerNoticiasJugadorAsync(int jugadorId)
        {
            _logger.LogInformation("Obteniendo noticias del jugador {JugadorId}", jugadorId);

            var noticias = await _noticiaRepository.ObtenerPorJugadorAsync(jugadorId);
            return noticias.Select(MapearAResponseDto).ToList();
        }

        public async Task<JugadorConNoticiasDto?> ObtenerJugadorConNoticiasAsync(int jugadorId)
        {
            _logger.LogInformation("Obteniendo jugador con noticias {JugadorId}", jugadorId);

            var jugador = await _noticiaRepository.ObtenerJugadorConNoticiasAsync(jugadorId);

            if (jugador == null)
            {
                return null;
            }

            return new JugadorConNoticiasDto
            {
                Id = jugador.Id,
                Nombre = jugador.Nombre,
                Posicion = jugador.Posicion,
                EquipoNFL = jugador.EquipoNFL?.Nombre ?? "Sin equipo",
                DesignacionLesion = jugador.DesignacionLesion,
                DesignacionDescripcion = ObtenerDescripcionDesignacion(jugador.DesignacionLesion),
                ImagenUrl = jugador.ImagenUrl,
                Noticias = jugador.Noticias.Select(MapearAResponseDto).ToList()
            };
        }

        public async Task<List<NoticiaJugadorResponseDto>> ObtenerTodasNoticiasAsync()
        {
            _logger.LogInformation("Obteniendo todas las noticias");

            var noticias = await _noticiaRepository.ObtenerTodasAsync();
            return noticias.Select(MapearAResponseDto).ToList();
        }

        public async Task<NoticiaJugadorResponseDto?> ObtenerNoticiaPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo noticia {NoticiaId}", id);

            var noticia = await _noticiaRepository.ObtenerPorIdAsync(id);

            if (noticia == null)
            {
                return null;
            }

            return MapearAResponseDto(noticia);
        }

        // Método privado para mapear entidad a DTO
        private NoticiaJugadorResponseDto MapearAResponseDto(NoticiaJugador noticia)
        {
            return new NoticiaJugadorResponseDto
            {
                Id = noticia.Id,
                JugadorId = noticia.JugadorId,
                NombreJugador = noticia.Jugador?.Nombre ?? "Desconocido",
                EquipoNFL = noticia.Jugador?.EquipoNFL?.Nombre ?? "Sin equipo",
                Texto = noticia.Texto,
                EsLesion = noticia.EsLesion,
                ResumenLesion = noticia.ResumenLesion,
                DesignacionLesion = noticia.DesignacionLesion,
                DesignacionDescripcion = ObtenerDescripcionDesignacion(noticia.DesignacionLesion),
                AutorId = noticia.AutorId,
                NombreAutor = noticia.Autor?.NombreCompleto ?? "Desconocido",
                FechaCreacion = noticia.FechaCreacion,
                Estado = noticia.Estado
            };
        }

        // Método privado para obtener la descripción de una designación
        private string? ObtenerDescripcionDesignacion(string? designacion)
        {
            if (string.IsNullOrWhiteSpace(designacion))
            {
                return null;
            }

            return designacion switch
            {
                "O" => "Fuera (Out) - No jugará",
                "D" => "Dudoso (Doubtful) - ~25% probabilidad de jugar",
                "Q" => "Cuestionable (Questionable) - ~50% probabilidad de jugar",
                "P" => "Probable (Probable) - Muy probable que juegue",
                "FP" => "Participación Plena (Full Practice) - Casi seguro que juega",
                "IR" => "Reserva de Lesionados (Injured Reserve) - Fuera por período extendido",
                "PUP" => "Incapaz Físicamente de Jugar (Physically Unable to Perform)",
                "SUS" => "Suspendido (Suspended) - No elegible por sanción",
                _ => designacion
            };
        }
    }
}
