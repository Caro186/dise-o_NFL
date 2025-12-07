using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Logic.Validators
{
    /// <summary>
    /// Validador centralizado para todas las validaciones relacionadas con temporadas
    /// REFACTORIZADO: Modularizado con métodos pequeños y claros, agrupados por responsabilidad
    /// </summary>
    public class TemporadaValidator
    {
        private readonly ITemporadaRepository _temporadaRepository;

        #region Constructor

        public TemporadaValidator(ITemporadaRepository temporadaRepository)
        {
            _temporadaRepository = temporadaRepository;
        }

        #endregion

        #region Validaciones de Datos Básicos

        /// <summary>
        /// Valida los campos requeridos para crear una temporada
        /// </summary>
        public void ValidarCamposRequeridos(string nombre, DateTime fechaInicio, DateTime fechaCierre)
        {
            ValidarNombreRequerido(nombre);
            ValidarFechasRequeridas(fechaInicio, fechaCierre);
        }

        /// <summary>
        /// Valida que el nombre no esté vacío
        /// </summary>
        public void ValidarNombreRequerido(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidationException("Nombre", "El nombre de la temporada es requerido");

            if (nombre.Length > 100)
                throw new ValidationException("Nombre", "El nombre de la temporada no puede exceder 100 caracteres");
        }

        /// <summary>
        /// Valida que las fechas estén presentes y sean válidas
        /// </summary>
        public void ValidarFechasRequeridas(DateTime fechaInicio, DateTime fechaCierre)
        {
            if (fechaInicio == default)
                throw new ValidationException("FechaInicio", "La fecha de inicio es requerida");

            if (fechaCierre == default)
                throw new ValidationException("FechaCierre", "La fecha de cierre es requerida");
        }

        #endregion

        #region Validaciones de Fechas

        /// <summary>
        /// Valida que la fecha de inicio sea anterior a la fecha de cierre
        /// </summary>
        public void ValidarRangoFechas(DateTime fechaInicio, DateTime fechaCierre)
        {
            if (fechaInicio >= fechaCierre)
                throw new ValidationException("Fechas", "La fecha de inicio debe ser anterior a la fecha de cierre");
        }

        /// <summary>
        /// Valida que las fechas no se traslapen con otra temporada existente
        /// </summary>
        public async Task ValidarSinTraslapeAsync(DateTime fechaInicio, DateTime fechaCierre)
        {
            if (await _temporadaRepository.HasOverlapAsync(fechaInicio, fechaCierre))
                throw new ValidationException("Fechas", "Las fechas se traslapan con otra temporada existente");
        }

        /// <summary>
        /// Valida que una semana esté dentro del rango de la temporada
        /// </summary>
        public void ValidarSemanaDentroDeTemporada(DateTime fechaInicioSemana, DateTime fechaFinSemana, DateTime fechaInicioTemporada, DateTime fechaCierreTemporada)
        {
            if (fechaInicioSemana < fechaInicioTemporada || fechaFinSemana > fechaCierreTemporada)
                throw new ValidationException("Semanas", "Las semanas deben estar dentro del rango de la temporada");
        }

        #endregion

        #region Validaciones de Duplicados

        /// <summary>
        /// Valida que no exista una temporada con el mismo nombre
        /// </summary>
        public async Task ValidarNoDuplicadoAsync(string nombre)
        {
            if (await _temporadaRepository.ExistsByNameAsync(nombre))
                throw new ValidationException("Nombre", "Ya existe una temporada con ese nombre");
        }

        #endregion

        #region Validaciones de Existencia

        /// <summary>
        /// Valida que la temporada existe
        /// </summary>
        public async Task<Temporada> ValidarTemporadaExisteAsync(int temporadaId)
        {
            var temporada = await _temporadaRepository.GetByIdAsync(temporadaId);
            if (temporada == null)
                throw new ValidationException("TemporadaId", $"Temporada con ID {temporadaId} no encontrada");

            return temporada;
        }

        #endregion

        #region Métodos Consolidados de Validación

        /// <summary>
        /// Valida todos los datos para crear una temporada (método consolidado)
        /// </summary>
        public async Task ValidarParaCrearAsync(CrearTemporadaDto dto)
        {
            // Validaciones de datos básicos
            ValidarCamposRequeridos(dto.Nombre, dto.FechaInicio, dto.FechaCierre);

            // Validaciones de fechas
            ValidarRangoFechas(dto.FechaInicio, dto.FechaCierre);

            // Validaciones de duplicados y traslapes
            await ValidarNoDuplicadoAsync(dto.Nombre);
            await ValidarSinTraslapeAsync(dto.FechaInicio, dto.FechaCierre);

            // Validaciones de semanas si se proporcionan
            if (dto.Semanas != null && dto.Semanas.Any())
            {
                foreach (var semana in dto.Semanas)
                {
                    ValidarSemanaDentroDeTemporada(semana.FechaInicio, semana.FechaFin, dto.FechaInicio, dto.FechaCierre);
                }
            }
        }

        #endregion
    }
}
