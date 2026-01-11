using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.Validators;
using NFLFantasyAPI.Logic.Exceptions;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace NFLFantasyAPI.Logic.Services
{
    public class EquipoFantasyService : IEquipoFantasyService
    {
        private readonly IEquipoFantasyRepository _repository;
        private readonly ILogger<EquipoFantasyService> _logger;
        private readonly EquipoFantasyValidator _validator;

        public EquipoFantasyService(IEquipoFantasyRepository repository, ILogger<EquipoFantasyService> logger, EquipoFantasyValidator validator)
        {
            _repository = repository;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ServiceResult> GetAllEquiposFantasyAsync()
        {
            try
            {
                var equipos = await _repository.GetAllAsync();

                var result = equipos.Select(e => new EquipoFantasyResponseDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    UsuarioId = e.UsuarioId,
                    NombrePropietario = e.Usuario?.NombreCompleto,
                    LigaId = e.LigaId,
                    NombreLiga = e.Liga?.NombreLiga,
                    ImagenUrl = e.ImagenUrl,
                    FechaCreacion = e.FechaCreacion,
                    Estado = e.Estado
                }).ToList();

                return ServiceResult.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipos fantasy");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> GetEquipoFantasyByIdAsync(int id)
        {
            try
            {
                await _validator.ValidarEquipoExisteAsync(id);
                var equipo = await _repository.GetByIdAsync(id);

            var dto = new EquipoFantasyResponseDto
            {
                Id = equipo.Id,
                Nombre = equipo.Nombre,
                UsuarioId = equipo.UsuarioId,
                NombrePropietario = equipo.Usuario?.NombreCompleto,
                LigaId = equipo.LigaId,
                NombreLiga = equipo.Liga?.NombreLiga,
                ImagenUrl = equipo.ImagenUrl,
                FechaCreacion = equipo.FechaCreacion,
                Estado = equipo.Estado
            };

            return ServiceResult.Ok(dto);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
        }

        public async Task<ServiceResult> GetEquiposByUsuarioAsync(int usuarioId)
        {
            var equipos = await _repository.GetByUsuarioIdAsync(usuarioId);

            var result = equipos.Select(e => new EquipoFantasyResponseDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                UsuarioId = e.UsuarioId,
                NombrePropietario = e.Usuario?.NombreCompleto,
                LigaId = e.LigaId,
                NombreLiga = e.Liga?.NombreLiga,
                ImagenUrl = e.ImagenUrl,
                FechaCreacion = e.FechaCreacion,
                Estado = e.Estado
            }).ToList();

            return ServiceResult.Ok(result);
        }

        public async Task<ServiceResult> CreateEquipoFantasyAsync(EquipoFantasyCreateDto dto)
        {
            try
            {
                // Validaciones usando el validador centralizado (método consolidado)
                await _validator.ValidarParaCrearAsync(dto);

                var equipo = new EquipoFantasy
                {
                    Nombre = dto.Nombre,
                    UsuarioId = dto.UsuarioId,
                    LigaId = dto.LigaId,
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Activo"
                };

                await _repository.AddAsync(equipo);
                await _repository.SaveChangesAsync();

                _logger.LogInformation("Equipo fantasy creado {Nombre}", equipo.Nombre);

                return ServiceResult.Ok(new { mensaje = "Equipo creado exitosamente", equipo.Id });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación al crear equipo fantasy: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear equipo fantasy");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen, string uploadsRoot, string baseUrl)
        {
            try
            {
                // Validar que el equipo existe
                await _validator.ValidarEquipoExisteAsync(id);

                // Validar que se proporcionó un archivo
                if (imagen == null)
                    throw new InvalidFileException("No se proporcionó ninguna imagen");

                // Validar el archivo usando el validador compartido
                FileValidator.ValidarArchivoImagen(imagen.ContentType, imagen.Length, maxSizeMB: 5);

                var equipo = await _repository.GetByIdAsync(id);

                var folder = Path.Combine(uploadsRoot, "uploads", "equipos-fantasy");
                Directory.CreateDirectory(folder);

                var extension = Path.GetExtension(imagen.FileName);
                var fileName = $"{id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await imagen.CopyToAsync(stream);

                equipo.ImagenUrl = $"{baseUrl}/uploads/equipos-fantasy/{fileName}";
                await _repository.SaveChangesAsync();

                return ServiceResult.Ok(new { mensaje = "Imagen actualizada", imagenUrl = equipo.ImagenUrl });
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
                _logger.LogError(ex, "Error al subir imagen del equipo {Id}", id);
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> DeleteEquipoFantasyAsync(int id)
        {
            try
            {
                await _validator.ValidarEquipoExisteAsync(id);
                var equipo = await _repository.GetByIdAsync(id);

            await _repository.DeleteAsync(equipo);
            await _repository.SaveChangesAsync();

            return ServiceResult.Ok(new { mensaje = "Equipo eliminado exitosamente" });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Error de validación: {ex.Message}");
                return ServiceResult.BadRequest(ex.Message);
            }
        }
    }
}
