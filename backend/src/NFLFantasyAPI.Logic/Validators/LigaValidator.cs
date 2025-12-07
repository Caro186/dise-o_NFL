using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Logic.Validators
{
    /// <summary>
    /// Validador centralizado para todas las validaciones relacionadas con ligas
    /// REFACTORIZADO: Modularizado con métodos pequeños y claros, agrupados por responsabilidad
    /// </summary>
    public class LigaValidator
    {
        private readonly ILigaRepository _ligaRepository;
        private readonly ITemporadaRepository _temporadaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        #region Constantes

        // Estados válidos para una liga
        private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pre-Draft", "Draft", "En Curso", "Finalizada"
        };

        // Rango válido de cupos para una liga
        private const int MIN_CUPOS = 2;
        private const int MAX_CUPOS = 20;

        #endregion

        #region Constructor

        public LigaValidator(
            ILigaRepository ligaRepository,
            ITemporadaRepository temporadaRepository,
            IUsuarioRepository usuarioRepository)
        {
            _ligaRepository = ligaRepository;
            _temporadaRepository = temporadaRepository;
            _usuarioRepository = usuarioRepository;
        }

        #endregion

        #region Validaciones de Datos Básicos

        /// <summary>
        /// Valida los campos requeridos para crear una liga
        /// </summary>
        public void ValidarCamposRequeridos(LigaCreateDto dto)
        {
            ValidarNombreLigaRequerido(dto.NombreLiga);
            ValidarPasswordRequerida(dto.PasswordHash);
            ValidarCuposTotales(dto.CuposTotales);
            ValidarComisionadoId(dto.ComisionadoId);
            ValidarTemporadaId(dto.IdTemporada);
        }

        /// <summary>
        /// Valida que el nombre de la liga no esté vacío
        /// </summary>
        public void ValidarNombreLigaRequerido(string nombreLiga)
        {
            if (string.IsNullOrWhiteSpace(nombreLiga))
                throw new ValidationException("NombreLiga", "El nombre de la liga es requerido");

            if (nombreLiga.Length > 100)
                throw new ValidationException("NombreLiga", "El nombre de la liga no puede exceder 100 caracteres");
        }

        /// <summary>
        /// Valida que la contraseña no esté vacía
        /// </summary>
        public void ValidarPasswordRequerida(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("PasswordHash", "La contraseña es requerida");

            if (password.Length > 255)
                throw new ValidationException("PasswordHash", "La contraseña no puede exceder 255 caracteres");
        }

        /// <summary>
        /// Valida que los cupos totales estén en el rango válido
        /// </summary>
        public void ValidarCuposTotales(int cuposTotales)
        {
            if (cuposTotales < MIN_CUPOS || cuposTotales > MAX_CUPOS)
                throw new ValidationException("CuposTotales",
                    $"Los cupos deben estar entre {MIN_CUPOS} y {MAX_CUPOS}. Valor recibido: {cuposTotales}");
        }

        /// <summary>
        /// Valida que el ID del comisionado sea válido
        /// </summary>
        public void ValidarComisionadoId(int comisionadoId)
        {
            if (comisionadoId <= 0)
                throw new ValidationException("ComisionadoId", "El ID del comisionado debe ser mayor a 0");
        }

        /// <summary>
        /// Valida que el ID de temporada sea válido
        /// </summary>
        public void ValidarTemporadaId(int temporadaId)
        {
            if (temporadaId <= 0)
                throw new ValidationException("IdTemporada", "El ID de temporada debe ser mayor a 0");
        }

        #endregion

        #region Validaciones de Estado

        /// <summary>
        /// Valida que el estado de la liga sea válido
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

        #endregion

        #region Validaciones de Relaciones

        /// <summary>
        /// Valida que la temporada existe
        /// </summary>
        public async Task ValidarTemporadaExisteAsync(int temporadaId)
        {
            var temporada = await _temporadaRepository.GetByIdAsync(temporadaId);
            if (temporada == null)
                throw new ValidationException("IdTemporada", $"Temporada con ID {temporadaId} no encontrada");
        }

        /// <summary>
        /// Valida que el comisionado existe
        /// </summary>
        public async Task ValidarComisionadoExisteAsync(int comisionadoId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(comisionadoId);
            if (usuario == null)
                throw new ValidationException("ComisionadoId", $"Usuario comisionado con ID {comisionadoId} no encontrado");
        }

        /// <summary>
        /// Valida que la liga existe
        /// </summary>
        public async Task<Liga> ValidarLigaExisteAsync(int ligaId)
        {
            var liga = await _ligaRepository.GetByIdAsync(ligaId);
            if (liga == null)
                throw new ValidationException("LigaId", "Liga no encontrada");

            return liga;
        }

        /// <summary>
        /// Valida que no exista una liga con el mismo nombre
        /// </summary>
        public async Task ValidarNoDuplicadoAsync(string nombreLiga, int? excludeId = null)
        {
            if (await _ligaRepository.ExistsByNombreAsync(nombreLiga, excludeId))
                throw new ValidationException("NombreLiga", "Ya existe una liga con ese nombre");
        }

        #endregion

        #region Validaciones de Unirse a Liga

        /// <summary>
        /// Valida la contraseña de la liga
        /// </summary>
        public void ValidarPasswordLiga(string password, string passwordHash)
        {
            if (!BCrypt.Net.BCrypt.Verify(password, passwordHash))
                throw new ValidationException("Password", "Contraseña incorrecta");
        }

        /// <summary>
        /// Valida que hay cupos disponibles en la liga
        /// </summary>
        public void ValidarCuposDisponibles(Liga liga)
        {
            if (liga.CuposOcupados >= liga.CuposTotales)
                throw new ValidationException("Cupos", "La liga está llena");
        }

        #endregion

        #region Validaciones de URLs

        /// <summary>
        /// Valida que la URL de imagen tenga formato válido
        /// </summary>
        public void ValidarUrlImagen(string? imagenUrl)
        {
            if (string.IsNullOrWhiteSpace(imagenUrl))
                return;

            if (imagenUrl.Length > 500)
                throw new ValidationException("ImagenUrl", "La URL de imagen no puede exceder 500 caracteres");

            if (!Uri.TryCreate(imagenUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException("ImagenUrl", $"La URL de imagen '{imagenUrl}' no tiene un formato válido");
            }
        }

        #endregion

        #region Métodos Consolidados de Validación

        /// <summary>
        /// Valida todos los datos para crear una liga (método consolidado)
        /// </summary>
        public async Task ValidarParaCrearAsync(LigaCreateDto dto)
        {
            // Validaciones de datos básicos
            ValidarCamposRequeridos(dto);

            // Validaciones de relaciones
            await ValidarTemporadaExisteAsync(dto.IdTemporada);
            await ValidarComisionadoExisteAsync(dto.ComisionadoId);

            // Validaciones de duplicados
            await ValidarNoDuplicadoAsync(dto.NombreLiga);
        }

        /// <summary>
        /// Valida todos los datos para unirse a una liga
        /// </summary>
        public async Task<Liga> ValidarParaUnirseAsync(UnirseLigaDto dto)
        {
            // Validar que la liga existe
            var liga = await ValidarLigaExisteAsync(dto.LigaId);

            // Validar contraseña
            ValidarPasswordLiga(dto.Password, liga.PasswordHash);

            // Validar cupos disponibles
            ValidarCuposDisponibles(liga);

            return liga;
        }

        #endregion
    }
}
