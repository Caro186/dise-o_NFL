using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
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

        public EquipoFantasyService(IEquipoFantasyRepository repository, ILogger<EquipoFantasyService> logger)
        {
            _repository = repository;
            _logger = logger;
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
            var equipo = await _repository.GetByIdAsync(id);
            if (equipo == null)
                return ServiceResult.BadRequest("Equipo fantasy no encontrado");

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
                if (!await _repository.UsuarioExisteAsync(dto.UsuarioId))
                    return ServiceResult.BadRequest("Usuario no encontrado");

                if (await _repository.NombreExisteAsync(dto.Nombre, dto.UsuarioId))
                    return ServiceResult.BadRequest("Ya tienes un equipo con ese nombre");

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
                var equipo = await _repository.GetByIdAsync(id);
                if (equipo == null)
                    return ServiceResult.BadRequest("Equipo no encontrado");

                if (imagen == null || imagen.Length == 0)
                    return ServiceResult.BadRequest("No se proporcionó ninguna imagen");

                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(imagen.ContentType.ToLower()))
                    return ServiceResult.BadRequest("Solo se permiten imágenes JPEG o PNG");

                if (imagen.Length > 5 * 1024 * 1024)
                    return ServiceResult.BadRequest("El tamaño máximo permitido es 5 MB");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen del equipo {Id}", id);
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> DeleteEquipoFantasyAsync(int id)
        {
            var equipo = await _repository.GetByIdAsync(id);
            if (equipo == null)
                return ServiceResult.BadRequest("Equipo no encontrado");

            await _repository.DeleteAsync(equipo);
            await _repository.SaveChangesAsync();

            return ServiceResult.Ok(new { mensaje = "Equipo eliminado exitosamente" });
        }
    }
}
