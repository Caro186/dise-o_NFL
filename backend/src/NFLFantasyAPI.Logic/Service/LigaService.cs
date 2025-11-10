using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.CrossCutting.Configuration;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Repositories;
using BCrypt.Net;

namespace NFLFantasyAPI.Logic.Services
{
    public class LigaService : ILigaService
    {
        private readonly ILigaRepository _ligaRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LigaService> _logger;
        private readonly FileServerSettings _fileServerSettings;

        public LigaService(
            ILigaRepository ligaRepository,
            IWebHostEnvironment environment,
            ILogger<LigaService> logger,
            IOptions<FileServerSettings> fileServerSettings)
        {
            _ligaRepository = ligaRepository;
            _environment = environment;
            _logger = logger;
            _fileServerSettings = fileServerSettings.Value;
        }

        public async Task<ServiceResult> GetAllAsync()
        {
            var ligas = await _ligaRepository.GetAllAsync();
            return ServiceResult.Ok(ligas);
        }

        public async Task<ServiceResult> GetByIdAsync(int id)
        {
            var liga = await _ligaRepository.GetByIdAsync(id);
            if (liga == null)
                return ServiceResult.BadRequest("Liga no encontrada");

            return ServiceResult.Ok(liga);
        }

        public async Task<ServiceResult> GetByComisionadoAsync(int usuarioId)
            => ServiceResult.Ok(await _ligaRepository.GetByComisionadoAsync(usuarioId));

        public async Task<ServiceResult> GetByUsuarioAsync(int usuarioId)
            => ServiceResult.Ok(await _ligaRepository.GetByUsuarioAsync(usuarioId));

        public async Task<ServiceResult> CreateAsync(LigaCreateDto dto)
        {
            // Ejemplo simplificado: deberías incluir validaciones como en tu controller original
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

        public async Task<ServiceResult> UpdateAsync(int id, LigaCreateDto dto)
        {
            var liga = await _ligaRepository.GetByIdAsync(id);
            if (liga == null)
                return ServiceResult.BadRequest("Liga no encontrada");

            liga.NombreLiga = dto.NombreLiga;
            liga.Descripcion = dto.Descripcion;

            await _ligaRepository.UpdateAsync(liga);
            await _ligaRepository.SaveChangesAsync();

            return ServiceResult.Ok(liga);
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var liga = await _ligaRepository.GetByIdAsync(id);
            if (liga == null)
                return ServiceResult.BadRequest("Liga no encontrada");

            await _ligaRepository.DeleteAsync(liga);
            await _ligaRepository.SaveChangesAsync();

            return ServiceResult.Ok(new { mensaje = "Liga eliminada exitosamente" });
        }

        public async Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen)
        {
            var liga = await _ligaRepository.GetByIdAsync(id);
            if (liga == null)
                return ServiceResult.BadRequest("Liga no encontrada");

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
    }
}
