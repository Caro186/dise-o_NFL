using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Logic.Validators
{
    /// <summary>
    /// Validador centralizado para todas las validaciones relacionadas con equipos NFL
    /// REFACTORIZADO: Modularizado con métodos pequeños y claros, agrupados por responsabilidad
    /// </summary>
    public class EquipoNFLValidator
    {
        private readonly IEquipoNFLRepository _equipoNFLRepository;

        #region Constantes

        // Estados válidos para un equipo NFL
        private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "Activo", "Inactivo"
        };

        #endregion

        #region Constructor

        public EquipoNFLValidator(IEquipoNFLRepository equipoNFLRepository)
        {
            _equipoNFLRepository = equipoNFLRepository;
        }

        #endregion

        #region Validaciones de Datos Básicos

        /// <summary>
        /// Valida los campos requeridos para crear un equipo NFL
        /// </summary>
        public void ValidarCamposRequeridos(string nombre, string ciudad)
        {
            ValidarNombreRequerido(nombre);
            ValidarCiudadRequerida(ciudad);
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
        /// Valida que la ciudad no esté vacía
        /// </summary>
        public void ValidarCiudadRequerida(string ciudad)
        {
            if (string.IsNullOrWhiteSpace(ciudad))
                throw new ValidationException("Ciudad", "La ciudad es requerida");

            if (ciudad.Length > 100)
                throw new ValidationException("Ciudad", "La ciudad no puede exceder 100 caracteres");
        }

        #endregion

        #region Validaciones de Estado

        /// <summary>
        /// Valida que el estado del equipo NFL sea válido
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

        #region Validaciones de Relaciones y Duplicados

        /// <summary>
        /// Valida que no exista un equipo NFL con el mismo nombre
        /// </summary>
        public async Task ValidarNoDuplicadoAsync(string nombre)
        {
            if (await _equipoNFLRepository.ExistsByNameAsync(nombre))
                throw new ValidationException("Nombre", "Ya existe un equipo NFL con ese nombre");
        }

        /// <summary>
        /// Valida que el equipo NFL existe
        /// </summary>
        public async Task ValidarEquipoExisteAsync(int equipoId)
        {
            var equipo = await _equipoNFLRepository.GetByIdAsync(equipoId);
            if (equipo == null)
                throw new EquipoNFLNotFoundException(equipoId);
        }

        /// <summary>
        /// Valida que el equipo NFL existe y está activo
        /// </summary>
        public async Task ValidarEquipoActivoAsync(int equipoId)
        {
            var equipo = await _equipoNFLRepository.GetByIdAsync(equipoId);
            if (equipo == null)
                throw new EquipoNFLNotFoundException(equipoId);

            if (!string.Equals(equipo.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Estado", $"El equipo NFL '{equipo.Nombre}' no está activo");
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
        /// Valida todos los datos para crear un equipo NFL (método consolidado)
        /// </summary>
        public async Task ValidarParaCrearAsync(EquipoNFLCreateDto dto)
        {
            // Validaciones de datos básicos
            ValidarCamposRequeridos(dto.Nombre, dto.Ciudad);

            // Validaciones de duplicados
            await ValidarNoDuplicadoAsync(dto.Nombre);
        }

        /// <summary>
        /// Valida todos los datos para actualizar un equipo NFL (método consolidado)
        /// </summary>
        public async Task ValidarParaActualizarAsync(EquipoNFL equipoExistente, string? nombre = null, string? ciudad = null, string? estado = null)
        {
            if (equipoExistente == null)
                throw new EquipoNFLNotFoundException(0);

            // Validar nombre si se proporciona
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                ValidarNombreRequerido(nombre);
                // Validar duplicado solo si el nombre cambió
                if (!string.Equals(nombre.Trim(), equipoExistente.Nombre.Trim(), StringComparison.OrdinalIgnoreCase))
                    await ValidarNoDuplicadoAsync(nombre);
            }

            // Validar ciudad si se proporciona
            if (!string.IsNullOrWhiteSpace(ciudad))
                ValidarCiudadRequerida(ciudad);

            // Validar estado si se proporciona
            if (!string.IsNullOrWhiteSpace(estado))
                ValidarEstadoValido(estado);
        }

        #endregion
    }
}
