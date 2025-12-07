using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Interfaces;

namespace NFLFantasyAPI.Logic.Validators
{
    /// <summary>
    /// Validador centralizado para todas las validaciones relacionadas con noticias de jugador
    /// REFACTORIZADO: Modularizado con métodos pequeños y claros, agrupados por responsabilidad
    /// </summary>
    public class NoticiaJugadorValidator
    {
        private readonly INoticiaJugadorRepository _noticiaRepository;

        #region Constantes

        // Designaciones válidas de lesión de la NFL
        private static readonly HashSet<string> DesignacionesValidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "O", "D", "Q", "P", "FP", "IR", "PUP", "SUS"
        };

        // Estados válidos para una noticia
        private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "Activa", "Inactiva"
        };

        #endregion

        #region Constructor

        public NoticiaJugadorValidator(INoticiaJugadorRepository noticiaRepository)
        {
            _noticiaRepository = noticiaRepository;
        }

        #endregion

        #region Validaciones de Datos Básicos

        /// <summary>
        /// Valida los campos requeridos para crear una noticia
        /// </summary>
        public void ValidarCamposRequeridos(CrearNoticiaJugadorDto dto)
        {
            ValidarJugadorIdRequerido(dto.JugadorId);
            ValidarTextoRequerido(dto.Texto);
            ValidarLongitudTexto(dto.Texto);
        }

        /// <summary>
        /// Valida que el ID del jugador sea válido
        /// </summary>
        public void ValidarJugadorIdRequerido(int jugadorId)
        {
            if (jugadorId <= 0)
                throw new ValidationException("JugadorId", "El ID del jugador debe ser mayor a 0");
        }

        /// <summary>
        /// Valida que el texto no esté vacío
        /// </summary>
        public void ValidarTextoRequerido(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                throw new ValidationException("Texto", "El texto de la noticia es requerido");
        }

        /// <summary>
        /// Valida la longitud del texto de la noticia (10-300 caracteres)
        /// </summary>
        public void ValidarLongitudTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return;

            if (texto.Length < 10)
                throw new ValidationException("Texto", "El texto debe tener al menos 10 caracteres");

            if (texto.Length > 300)
                throw new ValidationException("Texto", "El texto no puede exceder 300 caracteres");
        }

        #endregion

        #region Validaciones de Lesión

        /// <summary>
        /// Valida los campos específicos de una noticia de lesión
        /// </summary>
        public void ValidarNoticiaLesion(CrearNoticiaJugadorDto dto)
        {
            if (!dto.EsLesion)
                return;

            ValidarResumenLesionRequerido(dto.ResumenLesion);
            ValidarDesignacionLesionRequerida(dto.DesignacionLesion);
            ValidarDesignacionValida(dto.DesignacionLesion);
            ValidarLongitudResumenLesion(dto.ResumenLesion);
        }

        /// <summary>
        /// Valida que el resumen de lesión esté presente cuando es noticia de lesión
        /// </summary>
        public void ValidarResumenLesionRequerido(string? resumenLesion)
        {
            if (string.IsNullOrWhiteSpace(resumenLesion))
                throw new ValidationException("ResumenLesion", "El resumen de la lesión es obligatorio para noticias de lesión");
        }

        /// <summary>
        /// Valida que la designación de lesión esté presente cuando es noticia de lesión
        /// </summary>
        public void ValidarDesignacionLesionRequerida(string? designacionLesion)
        {
            if (string.IsNullOrWhiteSpace(designacionLesion))
                throw new ValidationException("DesignacionLesion", "La designación de lesión es obligatoria para noticias de lesión");
        }

        /// <summary>
        /// Valida que la designación de lesión sea válida
        /// </summary>
        public void ValidarDesignacionValida(string? designacion)
        {
            if (string.IsNullOrWhiteSpace(designacion))
                return;

            if (!DesignacionesValidas.Contains(designacion.Trim()))
            {
                throw new ValidationException("DesignacionLesion",
                    $"Designación de lesión inválida. Valores permitidos: {string.Join(", ", DesignacionesValidas)}");
            }
        }

        /// <summary>
        /// Valida la longitud del resumen de lesión (máximo 30 caracteres)
        /// </summary>
        public void ValidarLongitudResumenLesion(string? resumen)
        {
            if (string.IsNullOrWhiteSpace(resumen))
                return;

            if (resumen.Length > 30)
                throw new ValidationException("ResumenLesion", "El resumen de lesión no puede exceder 30 caracteres");
        }

        #endregion

        #region Validaciones de Relaciones

        /// <summary>
        /// Valida que el jugador existe y está activo
        /// </summary>
        public async Task ValidarJugadorExisteYActivoAsync(int jugadorId)
        {
            var jugadorExiste = await _noticiaRepository.ExisteJugadorAsync(jugadorId);
            if (!jugadorExiste)
                throw new InvalidOperationException("El jugador no existe o está inactivo");
        }

        #endregion

        #region Métodos Consolidados de Validación

        /// <summary>
        /// Valida todos los datos para crear una noticia (método consolidado)
        /// </summary>
        public async Task ValidarParaCrearAsync(CrearNoticiaJugadorDto dto)
        {
            // Validaciones de datos básicos
            ValidarCamposRequeridos(dto);

            // Validaciones de relación
            await ValidarJugadorExisteYActivoAsync(dto.JugadorId);

            // Validaciones específicas de lesión si aplica
            if (dto.EsLesion)
            {
                ValidarNoticiaLesion(dto);
            }
            else
            {
                // Si no es lesión, asegurar que no tenga campos de lesión
                if (!string.IsNullOrWhiteSpace(dto.ResumenLesion))
                    throw new ValidationException("ResumenLesion", "El resumen de lesión solo se permite en noticias de lesión");

                if (!string.IsNullOrWhiteSpace(dto.DesignacionLesion))
                    throw new ValidationException("DesignacionLesion", "La designación de lesión solo se permite en noticias de lesión");
            }
        }

        #endregion

        #region Métodos de Utilidad

        /// <summary>
        /// Obtiene la descripción de una designación de lesión
        /// </summary>
        public static string? ObtenerDescripcionDesignacion(string? designacion)
        {
            if (string.IsNullOrWhiteSpace(designacion))
                return null;

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

        #endregion
    }
}
