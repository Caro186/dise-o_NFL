using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Logic.Validators
{
    /// <summary>
    /// Validador centralizado para todas las validaciones relacionadas con equipos Fantasy
    /// REFACTORIZADO: Modularizado con métodos pequeños y claros, agrupados por responsabilidad
    /// </summary>
    public class EquipoFantasyValidator
    {
        private readonly IEquipoFantasyRepository _equipoFantasyRepository;

        #region Constantes

        // Estados válidos para un equipo Fantasy
        private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "Activo", "Inactivo"
        };

        #endregion

        #region Constructor

        public EquipoFantasyValidator(IEquipoFantasyRepository equipoFantasyRepository)
        {
            _equipoFantasyRepository = equipoFantasyRepository;
        }

        #endregion

        #region Validaciones de Datos Básicos

        /// <summary>
        /// Valida los campos requeridos para crear un equipo Fantasy
        /// </summary>
        public void ValidarCamposRequeridos(string nombre, int usuarioId)
        {
            ValidarNombreRequerido(nombre);
            ValidarUsuarioIdRequerido(usuarioId);
        }

        /// <summary>
        /// Valida que el nombre no esté vacío
        /// </summary>
        public void ValidarNombreRequerido(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidationException("Nombre", "El nombre del equipo es requerido");

            if (nombre.Length > 100)
                throw new ValidationException("Nombre", "El nombre del equipo no puede exceder 100 caracteres");
        }

        /// <summary>
        /// Valida que el ID del usuario sea válido
        /// </summary>
        public void ValidarUsuarioIdRequerido(int usuarioId)
        {
            if (usuarioId <= 0)
                throw new ValidationException("UsuarioId", "El ID del usuario debe ser mayor a 0");
        }

        #endregion

        #region Validaciones de Estado

        /// <summary>
        /// Valida que el estado del equipo Fantasy sea válido
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
        /// Valida que el usuario existe
        /// </summary>
        public async Task ValidarUsuarioExisteAsync(int usuarioId)
        {
            if (!await _equipoFantasyRepository.UsuarioExisteAsync(usuarioId))
                throw new ValidationException("UsuarioId", "Usuario no encontrado");
        }

        /// <summary>
        /// Valida que no exista un equipo Fantasy con el mismo nombre para el mismo usuario
        /// </summary>
        public async Task ValidarNoDuplicadoAsync(string nombre, int usuarioId)
        {
            if (await _equipoFantasyRepository.NombreExisteAsync(nombre, usuarioId))
                throw new ValidationException("Nombre", "Ya tienes un equipo con ese nombre");
        }

        /// <summary>
        /// Valida que el equipo Fantasy existe
        /// </summary>
        public async Task ValidarEquipoExisteAsync(int equipoId)
        {
            var equipo = await _equipoFantasyRepository.GetByIdAsync(equipoId);
            if (equipo == null)
                throw new ValidationException("EquipoId", "Equipo fantasy no encontrado");
        }

        /// <summary>
        /// Valida que el equipo pertenece al usuario
        /// </summary>
        public async Task ValidarEquipoPerteneceAUsuarioAsync(int equipoId, int usuarioId)
        {
            var equipo = await _equipoFantasyRepository.GetByIdAsync(equipoId);
            if (equipo == null)
                throw new ValidationException("EquipoId", "Equipo fantasy no encontrado");

            if (equipo.UsuarioId != usuarioId)
                throw new ValidationException("EquipoId", "El equipo no pertenece al usuario");
        }

        /// <summary>
        /// Valida que el equipo no esté ya en otra liga
        /// </summary>
        public async Task ValidarEquipoNoEstáEnLigaAsync(int equipoId)
        {
            var equipo = await _equipoFantasyRepository.GetByIdAsync(equipoId);
            if (equipo == null)
                throw new ValidationException("EquipoId", "Equipo fantasy no encontrado");

            if (equipo.LigaId.HasValue)
                throw new ValidationException("EquipoId", "El equipo ya está en otra liga");
        }

        /// <summary>
        /// Valida que el usuario no tenga ya un equipo en la liga
        /// </summary>
        public async Task ValidarUsuarioNoTieneEquipoEnLigaAsync(int usuarioId, int ligaId)
        {
            var equipos = await _equipoFantasyRepository.GetByUsuarioIdAsync(usuarioId);
            if (equipos != null && equipos.Any(e => e.LigaId == ligaId))
                throw new ValidationException("LigaId", "Ya tienes un equipo en esta liga");
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
        /// Valida todos los datos para crear un equipo Fantasy (método consolidado)
        /// </summary>
        public async Task ValidarParaCrearAsync(EquipoFantasyCreateDto dto)
        {
            // Validaciones de datos básicos
            ValidarCamposRequeridos(dto.Nombre, dto.UsuarioId);

            // Validaciones de relaciones
            await ValidarUsuarioExisteAsync(dto.UsuarioId);
            await ValidarNoDuplicadoAsync(dto.Nombre, dto.UsuarioId);
        }

        /// <summary>
        /// Valida todos los datos para unirse a una liga
        /// </summary>
        public async Task ValidarParaUnirseALigaAsync(int equipoId, int usuarioId)
        {
            // Validar que el equipo existe
            await ValidarEquipoExisteAsync(equipoId);

            // Validar que el equipo pertenece al usuario
            await ValidarEquipoPerteneceAUsuarioAsync(equipoId, usuarioId);

            // Validar que el equipo no esté ya en otra liga
            await ValidarEquipoNoEstáEnLigaAsync(equipoId);
        }

        #endregion
    }
}
