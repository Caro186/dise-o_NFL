using Microsoft.Extensions.Logging;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.Validators;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Logic.Service
{
    public class NoticiaJugadorService : INoticiaJugadorService
    {
        private readonly INoticiaJugadorRepository _noticiaRepository;
        private readonly ILogger<NoticiaJugadorService> _logger;
        private readonly NoticiaJugadorValidator _validator;

        public NoticiaJugadorService(
            INoticiaJugadorRepository noticiaRepository,
            ILogger<NoticiaJugadorService> logger,
            NoticiaJugadorValidator validator)
        {
            _noticiaRepository = noticiaRepository;
            _logger = logger;
            _validator = validator;
        }

        public async Task<NoticiaJugadorResponseDto> CrearNoticiaAsync(CrearNoticiaJugadorDto dto, int autorId)
        {
            _logger.LogInformation("Creando noticia para jugador {JugadorId}", dto.JugadorId);

            // Validaciones usando el validador centralizado (método consolidado)
            await _validator.ValidarParaCrearAsync(dto);

            // Si no es lesión, limpiar campos de lesión
            if (!dto.EsLesion)
            {
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
                DesignacionDescripcion = NoticiaJugadorValidator.ObtenerDescripcionDesignacion(jugador.DesignacionLesion),
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
                DesignacionDescripcion = NoticiaJugadorValidator.ObtenerDescripcionDesignacion(noticia.DesignacionLesion),
                AutorId = noticia.AutorId,
                NombreAutor = noticia.Autor?.NombreCompleto ?? "Desconocido",
                FechaCreacion = noticia.FechaCreacion,
                Estado = noticia.Estado
            };
        }

    }
}
