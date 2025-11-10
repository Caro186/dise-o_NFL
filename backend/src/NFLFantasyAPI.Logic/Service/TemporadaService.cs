using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace NFLFantasyAPI.Logic.Service
{
    public class TemporadaService : ITemporadaService
    {
        private readonly ITemporadaRepository _temporadaRepo;
        private readonly ILogger<TemporadaService> _logger;

        public TemporadaService(ITemporadaRepository temporadaRepo, ILogger<TemporadaService> logger)
        {
            _temporadaRepo = temporadaRepo;
            _logger = logger;
        }

        public async Task<ServiceResult> CrearTemporadaAsync(CrearTemporadaDto dto)
        {
            if (await _temporadaRepo.ExistsByNameAsync(dto.Nombre))
                return ServiceResult.BadRequest("Ya existe una temporada con ese nombre");

            if (dto.FechaInicio >= dto.FechaCierre)
                return ServiceResult.BadRequest("La fecha de inicio debe ser anterior a la de cierre");

            if (await _temporadaRepo.HasOverlapAsync(dto.FechaInicio, dto.FechaCierre))
                return ServiceResult.BadRequest("Las fechas se traslapan con otra temporada existente");

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

            // Crear semanas
            if (dto.Semanas != null)
            {
                foreach (var s in dto.Semanas)
                {
                    if (s.FechaInicio < temporada.FechaInicio || s.FechaFin > temporada.FechaCierre)
                        return ServiceResult.BadRequest("Las semanas deben estar dentro del rango de la temporada");

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
            var temporada = await _temporadaRepo.GetByIdAsync(id);
            if (temporada == null)
                return ServiceResult.BadRequest("Temporada no encontrada");

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

        public async Task<ServiceResult> MarcarComoActualAsync(int id)
        {
            var temporada = await _temporadaRepo.GetByIdAsync(id);
            if (temporada == null)
                return ServiceResult.BadRequest("Temporada no encontrada");

            var actual = await _temporadaRepo.GetActualesAsync();
            if (actual != null) actual.Actual = false;

            temporada.Actual = true;
            await _temporadaRepo.SaveChangesAsync();

            _logger.LogInformation("Temporada {Id} marcada como actual", id);
            return ServiceResult.Ok(new { mensaje = "Temporada marcada como actual" });
        }
    }
}
