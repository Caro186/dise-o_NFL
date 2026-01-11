using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.CrossCutting.Configuration;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Validators;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using BCrypt.Net;
using static NFLFantasyAPI.Logic.Validators.FileValidator;

namespace NFLFantasyAPI.Logic.Services
{
    public class LigaService : ILigaService
    {
        private readonly ILigaRepository _ligaRepository;
        private readonly IUsuarioRepository _usuarioRespository;
        private readonly IEquipoFantasyRepository _equipoFantasyRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LigaService> _logger;
        private readonly FileServerSettings _fileServerSettings;
        private readonly LigaValidator _ligaValidator;
        private readonly EquipoFantasyValidator _equipoFantasyValidator;
        private readonly ITemporadaRepository _temporadaRepository;


        public LigaService(
            ILigaRepository ligaRepository,
            IUsuarioRepository usuarioRespository,
            IEquipoFantasyRepository equipoFantasyRepository,
            IWebHostEnvironment environment,
            ILogger<LigaService> logger,
            IOptions<FileServerSettings> fileServerSettings,
            LigaValidator ligaValidator,
            EquipoFantasyValidator equipoFantasyValidator,
            ITemporadaRepository temporadaRepository)
        {
            _ligaRepository = ligaRepository;
            _environment = environment;
            _logger = logger;
            _fileServerSettings = fileServerSettings.Value;
            _usuarioRespository = usuarioRespository;
            _equipoFantasyRepository = equipoFantasyRepository;
            _ligaValidator = ligaValidator;
            _equipoFantasyValidator = equipoFantasyValidator;
            _temporadaRepository = temporadaRepository;
        }

        public async Task<ServiceResult> GetAllAsync()
        {
            var ligas = await _ligaRepository.GetAllAsync();

            var ligasResponse = ligas.Select(l => new LigaResponseDto
            {
                IdLiga = l.IdLiga,
                ImagenUrl = l.ImagenUrl,
                NombreLiga = l.NombreLiga,
                Descripcion = l.Descripcion,
                IdTemporada = l.IdTemporada,
                NombreTemporada = l.Temporada?.Nombre,
                Estado = l.Estado,
                CuposTotales = l.CuposTotales,
                CuposOcupados = l.CuposOcupados,
                FechaCreacion = l.FechaCreacion,
                FechaInicio = l.FechaInicio,
                FechaFin = l.FechaFin,
                ComisionadoId = l.ComisionadoId,
                NombreComisionado = l.Comisionado?.NombreCompleto,
                FormatoPosiciones = l.FormatoPosiciones,
                EsquemaPuntos = l.EsquemaPuntos,
                ConfigPlayoffs = l.ConfigPlayoffs,
                PermitirDecimales = l.PermitirDecimales
            }).ToList();

            return ServiceResult.Ok(ligasResponse);
        }

        public async Task<ServiceResult> GetByIdAsync(int id)
        {
            try
            {
                var liga = await _ligaValidator.ValidarLigaExisteAsync(id);
                return ServiceResult.Ok(liga);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
        }

        public async Task<ServiceResult> GetByComisionadoAsync(int usuarioId)
            => ServiceResult.Ok(await _ligaRepository.GetByComisionadoAsync(usuarioId));

        public async Task<ServiceResult> GetByUsuarioAsync(int usuarioId)
        {
            var ligas = await _ligaRepository.GetByUsuarioAsync(usuarioId);

            if (ligas == null || ligas.Count == 0)
                return ServiceResult.Ok(new List<LigaResponseDto>()); // o BadRequest si prefieres indicar "sin ligas"

            var ligasDto = ligas.Select(l => new LigaResponseDto
            {
                IdLiga = l.IdLiga,
                ImagenUrl = l.ImagenUrl,
                NombreLiga = l.NombreLiga,
                Descripcion = l.Descripcion,
                IdTemporada = l.IdTemporada,
                NombreTemporada = l.Temporada != null ? l.Temporada.Nombre : null,
                Estado = l.Estado,
                CuposTotales = l.CuposTotales,
                CuposOcupados = l.CuposOcupados,
                FechaCreacion = l.FechaCreacion,
                FechaInicio = l.FechaInicio,
                FechaFin = l.FechaFin,
                ComisionadoId = l.ComisionadoId,
                NombreComisionado = l.Comisionado != null ? l.Comisionado.NombreCompleto : null,
                FormatoPosiciones = l.FormatoPosiciones,
                EsquemaPuntos = l.EsquemaPuntos,
                ConfigPlayoffs = l.ConfigPlayoffs,
                PermitirDecimales = l.PermitirDecimales
            }).ToList();

            return ServiceResult.Ok(ligasDto);
        }

        public async Task<ServiceResult> CreateAsync(LigaCreateDto dto)
        {
            try
            {
                // Validaciones usando el validador centralizado (método consolidado)
                await _ligaValidator.ValidarParaCrearAsync(dto);

                var liga = new Liga
                {
                    NombreLiga = dto.NombreLiga,
                    Descripcion = dto.Descripcion,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash),
                    IdTemporada = dto.IdTemporada,
                    Estado = "Pre-Draft",
                    CuposTotales = dto.CuposTotales,
                    CuposOcupados = 1,
                    FechaCreacion = DateTime.UtcNow,
                    ComisionadoId = dto.ComisionadoId
                };

                await _ligaRepository.AddAsync(liga);
                await _ligaRepository.SaveChangesAsync();

                _logger.LogInformation("Liga creada {Nombre}", liga.NombreLiga);
                return ServiceResult.Ok(liga);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación al crear liga: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear liga");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> UpdateAsync(int id, LigaCreateDto dto)
        {
            try
            {
                var liga = await _ligaValidator.ValidarLigaExisteAsync(id);

                liga.NombreLiga = dto.NombreLiga;
                liga.Descripcion = dto.Descripcion;

                await _ligaRepository.UpdateAsync(liga);
                await _ligaRepository.SaveChangesAsync();

                return ServiceResult.Ok(liga);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar liga");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var liga = await _ligaValidator.ValidarLigaExisteAsync(id);

                await _ligaRepository.DeleteAsync(liga);
                await _ligaRepository.SaveChangesAsync();

                return ServiceResult.Ok(new { mensaje = "Liga eliminada exitosamente" });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar liga");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen)
        {
            try
            {
                var liga = await _ligaValidator.ValidarLigaExisteAsync(id);

                if (imagen == null)
                    throw new InvalidFileException("No se proporcionó ninguna imagen");

                FileValidator.ValidarArchivoImagen(imagen.ContentType, imagen.Length, maxSizeMB: 5);

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ligas");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(imagen.FileName);
                var fileName = $"{id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                liga.ImagenUrl = $"{_fileServerSettings.BaseUrl}/uploads/ligas/{fileName}";
                await _ligaRepository.SaveChangesAsync();

                return ServiceResult.Ok(new { imagenUrl = liga.ImagenUrl });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (InvalidFileException ex)
            {
                _logger.LogWarning(ex.Message);
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen de liga");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> UnirseLigaAsync(UnirseLigaDto dto)
        {
            try
            {
                // 1-3. Validaciones de liga usando el validador consolidado
                var liga = await _ligaValidator.ValidarParaUnirseAsync(dto);

                // 4. Validar usuario
                var usuario = await _usuarioRespository.GetByIdAsync(dto.UsuarioId);
                if (usuario == null)
                    throw new ValidationException("UsuarioId", "Usuario no encontrado");

                // 5-6. Validaciones de equipo usando el validador consolidado
                await _equipoFantasyValidator.ValidarParaUnirseALigaAsync(dto.EquipoId, dto.UsuarioId);
                await _equipoFantasyValidator.ValidarUsuarioNoTieneEquipoEnLigaAsync(dto.UsuarioId, dto.LigaId);

                // 7. Obtener el equipo y actualizar entidades
                var equipoFantasy = await _equipoFantasyRepository.GetByIdAsync(dto.EquipoId);
                if (equipoFantasy == null)
                    throw new ValidationException("EquipoId", "Equipo fantasy no encontrado");

                equipoFantasy.LigaId = liga.IdLiga;
                liga.CuposOcupados++;

                await _equipoFantasyRepository.UpdateAsync(equipoFantasy);
                await _ligaRepository.UpdateAsync(liga);
                await _ligaRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Usuario {UsuarioId} se unió a liga {LigaId} con equipo {EquipoId}",
                    dto.UsuarioId, dto.LigaId, dto.EquipoId);

                return ServiceResult.Ok(new
                {
                    mensaje = $"Te has unido exitosamente a la liga '{liga.NombreLiga}'",
                    ligaId = liga.IdLiga,
                    nombreLiga = liga.NombreLiga,
                    equipoId = equipoFantasy.Id,
                    alias = dto.Alias
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación al unirse a liga: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unirse a liga");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

    }
}
