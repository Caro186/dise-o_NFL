using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.Validators;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace NFLFantasyAPI.Logic.Service
{
    public class TemporadaService : ITemporadaService
    {
        private readonly ITemporadaRepository _temporadaRepo;
        private readonly ILogger<TemporadaService> _logger;
        private readonly TemporadaValidator _validator;

        public TemporadaService(ITemporadaRepository temporadaRepo, ILogger<TemporadaService> logger, TemporadaValidator validator)
        {
            _temporadaRepo = temporadaRepo;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ServiceResult> CrearTemporadaAsync(CrearTemporadaDto dto)
        {
            try
            {
                // Validaciones usando el validador centralizado (método consolidado)
                await _validator.ValidarParaCrearAsync(dto);

                if (dto.Actual)
                {
                    var actual = await _temporadaRepo.GetActualesAsync();
                    if (actual != null) actual.Actual = false;
                }

                var temporada = new Temporada
                {
                    Nombre = dto.Nombre,
                    FechaInicio = dto.FechaInicio,
                    FechaCierre = dto.FechaCierre,
                    FechaCreacion = DateTime.UtcNow,
                    Actual = dto.Actual
                };

                await _temporadaRepo.AddAsync(temporada);
                await _temporadaRepo.SaveChangesAsync();

                // Crear semanas (las validaciones de semanas ya están en el validador)
                if (dto.Semanas != null && dto.Semanas.Any())
                {
                    foreach (var s in dto.Semanas)
                    {
                        temporada.Semanas.Add(new Semana
                        {
                            FechaInicio = s.FechaInicio,
                            FechaFin = s.FechaFin,
                            Temporada = temporada
                        });
                    }

                    await _temporadaRepo.SaveChangesAsync();
                }

                _logger.LogInformation("Temporada creada: {Nombre}", temporada.Nombre);

                var response = new TemporadaResponseDto
                {
                    Id = temporada.Id,
                    Nombre = temporada.Nombre,
                    FechaInicio = temporada.FechaInicio,
                    FechaCierre = temporada.FechaCierre,
                    FechaCreacion = temporada.FechaCreacion,
                    Actual = temporada.Actual
                };

                return ServiceResult.Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación al crear temporada: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear temporada");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> ObtenerTemporadasAsync()
        {
            var temporadas = await _temporadaRepo.GetAllAsync();
            var data = temporadas.Select(t => new TemporadaResponseDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                FechaInicio = t.FechaInicio,
                FechaCierre = t.FechaCierre,
                FechaCreacion = t.FechaCreacion,
                Actual = t.Actual
            }).ToList();

            return ServiceResult.Ok(data);
        }

        public async Task<ServiceResult> ObtenerTemporadaAsync(int id)
        {
            try
            {
                var temporada = await _validator.ValidarTemporadaExisteAsync(id);

                var dto = new TemporadaResponseDto
                {
                    Id = temporada.Id,
                    Nombre = temporada.Nombre,
                    FechaInicio = temporada.FechaInicio,
                    FechaCierre = temporada.FechaCierre,
                    FechaCreacion = temporada.FechaCreacion,
                    Actual = temporada.Actual
                };

                return ServiceResult.Ok(dto);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
        }

        public async Task<ServiceResult> MarcarComoActualAsync(int id)
        {
            try
            {
                var temporada = await _validator.ValidarTemporadaExisteAsync(id);

                var actual = await _temporadaRepo.GetActualesAsync();
                if (actual != null) actual.Actual = false;

                temporada.Actual = true;
                await _temporadaRepo.SaveChangesAsync();

                _logger.LogInformation("Temporada {Id} marcada como actual", id);
                return ServiceResult.Ok(new { mensaje = "Temporada marcada como actual" });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar temporada como actual");
                return ServiceResult.Error("Error interno del servidor");
            }
        }
    }
}
