using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.CrossCutting.Configuration;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NFLFantasyAPI.Logic.Services
{
    public class EquipoNFLService : IEquipoNFLService
    {
        private readonly IEquipoNFLRepository _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EquipoNFLService> _logger;
        private readonly FileServerSettings _fileServerSettings;

        public EquipoNFLService(
            IEquipoNFLRepository repository,
            IWebHostEnvironment environment,
            ILogger<EquipoNFLService> logger,
            IOptions<FileServerSettings> fileServerSettings)
        {
            _repository = repository;
            _environment = environment;
            _logger = logger;
            _fileServerSettings = fileServerSettings.Value;
        }

        public async Task<ServiceResult> GetAllAsync()
        {
            try
            {
                var equipos = await _repository.GetAllAsync();
                var response = equipos.Select(e => new EquipoNFLResponseDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Ciudad = e.Ciudad,
                    ImagenUrl = e.ImagenUrl,
                    FechaCreacion = e.FechaCreacion,
                    Estado = e.Estado
                }).ToList();

                return ServiceResult.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipos NFL");
                return ServiceResult.Error("Error interno del servidor al obtener equipos NFL");
            }
        }

        public async Task<ServiceResult> GetByIdAsync(int id)
        {
            try
            {
                var equipo = await _repository.GetByIdAsync(id);
                if (equipo == null)
                    return ServiceResult.BadRequest("Equipo NFL no encontrado");

                var response = new EquipoNFLResponseDto
                {
                    Id = equipo.Id,
                    Nombre = equipo.Nombre,
                    Ciudad = equipo.Ciudad,
                    ImagenUrl = equipo.ImagenUrl,
                    FechaCreacion = equipo.FechaCreacion,
                    Estado = equipo.Estado
                };

                return ServiceResult.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener equipo NFL {Id}", id);
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> CreateAsync(EquipoNFLCreateDto equipoDto)
        {
            try
            {
                if (await _repository.ExistsByNameAsync(equipoDto.Nombre))
                    return ServiceResult.BadRequest("Ya existe un equipo NFL con ese nombre");

                var equipo = new EquipoNFL
                {
                    Nombre = equipoDto.Nombre,
                    Ciudad = equipoDto.Ciudad,
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Activo"
                };

                await _repository.AddAsync(equipo);
                await _repository.SaveChangesAsync();

                return ServiceResult.Ok(new EquipoNFLResponseDto
                {
                    Id = equipo.Id,
                    Nombre = equipo.Nombre,
                    Ciudad = equipo.Ciudad,
                    FechaCreacion = equipo.FechaCreacion,
                    Estado = equipo.Estado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear equipo NFL");
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen)
        {
            try
            {
                var equipo = await _repository.GetByIdAsync(id);
                if (equipo == null)
                    return ServiceResult.BadRequest("Equipo NFL no encontrado");

                if (imagen == null || imagen.Length == 0)
                    return ServiceResult.BadRequest("No se proporcionó ninguna imagen");

                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(imagen.ContentType.ToLower()))
                    return ServiceResult.BadRequest("Solo se permiten imágenes JPEG o PNG");

                if (imagen.Length > 5 * 1024 * 1024)
                    return ServiceResult.BadRequest("El tamaño máximo permitido es 5 MB");

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "equipos-nfl");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(imagen.FileName);
                var fileName = $"{id}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await imagen.CopyToAsync(stream);

                equipo.ImagenUrl = $"{_fileServerSettings.BaseUrl}/uploads/equipos-nfl/{fileName}";
                await _repository.SaveChangesAsync();

                return ServiceResult.Ok(new
                {
                    mensaje = "Imagen actualizada exitosamente",
                    imagenUrl = equipo.ImagenUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen del equipo NFL {Id}", id);
                return ServiceResult.Error("Error interno del servidor");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var equipo = await _repository.GetByIdAsync(id);
                if (equipo == null)
                    return ServiceResult.BadRequest("Equipo NFL no encontrado");

                await _repository.DeleteAsync(equipo);
                await _repository.SaveChangesAsync();

                return ServiceResult.Ok(new { mensaje = "Equipo NFL eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar equipo NFL {Id}", id);
                return ServiceResult.Error("Error interno del servidor");
            }
        }
    }
}
