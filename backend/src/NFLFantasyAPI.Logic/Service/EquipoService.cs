using Microsoft.Extensions.Logging;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.CrossCutting;

namespace NFLFantasyAPI.Logic.Services
{
    public class EquipoService : IEquipoService
    {
        private readonly IEquipoRepository _equipoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ILogger<EquipoService> _logger;

        public EquipoService(
            IEquipoRepository equipoRepo,
            IUsuarioRepository usuarioRepo,
            ILogger<EquipoService> logger)
        {
            _equipoRepo = equipoRepo;
            _usuarioRepo = usuarioRepo;
            _logger = logger;
        }

        public async Task<ServiceResult> CreateEquipoAsync(EquipoCreateDto dto)
        {
            if (await _usuarioRepo.GetByIdAsync(dto.UsuarioId) == null)
                return ServiceResult.BadRequest("Usuario no encontrado");

            if (await _equipoRepo.ExistsByNombreAsync(dto.Nombre, dto.UsuarioId))
                return ServiceResult.BadRequest("Ya existe un equipo con ese nombre para este usuario");

            var equipo = new Equipo
            {
                Nombre = dto.Nombre,
                UsuarioId = dto.UsuarioId,
                Liga = dto.Liga,
                FechaCreacion = DateTime.UtcNow,
                Estado = "Activo"
            };

            await _equipoRepo.AddAsync(equipo);
            await _equipoRepo.SaveChangesAsync();

            _logger.LogInformation("Equipo creado: {Nombre}", equipo.Nombre);

            var usuario = await _usuarioRepo.GetByIdAsync(dto.UsuarioId);

            var response = new EquipoResponseDto
            {
                Id = equipo.Id,
                Nombre = equipo.Nombre,
                UsuarioId = equipo.UsuarioId,
                FechaCreacion = equipo.FechaCreacion,
                NombrePropietario = usuario?.NombreCompleto,
                Estado = equipo.Estado,
                Liga = equipo.Liga
            };

            return ServiceResult.Ok(response);
        }

        public async Task<ServiceResult> GetEquipoAsync(int id)
        {
            var equipo = await _equipoRepo.GetByIdAsync(id);
            if (equipo == null)
                return ServiceResult.BadRequest("Equipo no encontrado");

            var response = new EquipoResponseDto
            {
                Id = equipo.Id,
                Nombre = equipo.Nombre,
                ImagenUrl = equipo.ImagenUrl,
                FechaCreacion = equipo.FechaCreacion,
                UsuarioId = equipo.UsuarioId,
                NombrePropietario = equipo.Usuario?.NombreCompleto,
                Estado = equipo.Estado,
                Liga = equipo.Liga
            };

            return ServiceResult.Ok(response);
        }

        public async Task<ServiceResult> GetEquiposByUsuarioAsync(int usuarioId)
        {
            var equipos = await _equipoRepo.GetByUsuarioAsync(usuarioId);
            var list = equipos.Select(e => new EquipoResponseDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                ImagenUrl = e.ImagenUrl,
                FechaCreacion = e.FechaCreacion,
                UsuarioId = e.UsuarioId,
                NombrePropietario = e.Usuario?.NombreCompleto,
                Estado = e.Estado,
                Liga = e.Liga
            }).ToList();

            return ServiceResult.Ok(list);
        }

        public async Task<ServiceResult> GetAllEquiposAsync()
        {
            var equipos = await _equipoRepo.GetAllAsync();
            var list = equipos.Select(e => new EquipoResponseDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                ImagenUrl = e.ImagenUrl,
                FechaCreacion = e.FechaCreacion,
                UsuarioId = e.UsuarioId,
                NombrePropietario = e.Usuario?.NombreCompleto,
                Estado = e.Estado,
                Liga = e.Liga
            }).ToList();

            return ServiceResult.Ok(list);
        }

        //public async Task<ServiceResult> UploadImagenAsync(int id, IFormFile imagen, string webRootPath)
        //Es necesario implementar el metodo de imagenes
        public async Task<ServiceResult> UploadImagenAsync(int id, string webRootPath)
        {
            var equipo = await _equipoRepo.GetByIdAsync(id);
            if (equipo == null)
                return ServiceResult.BadRequest("Equipo no encontrado");

            /* // Imagen
            if (imagen == null || imagen.Length == 0)
                return ServiceResult.BadRequest("No se proporcionó ninguna imagen");

            var allowedTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedTypes.Contains(imagen.ContentType.ToLower()))
                return ServiceResult.BadRequest("Solo se permiten imágenes JPEG o PNG");

            if (imagen.Length > 5 * 1024 * 1024)
                return ServiceResult.BadRequest("El tamaño máximo permitido es 5 MB");

            var uploadsFolder = Path.Combine(webRootPath, "uploads", "equipos");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imagen.FileName);
            var fileName = $"{id}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await imagen.CopyToAsync(stream);

            equipo.ImagenUrl = $"/uploads/equipos/{fileName}";
            await _equipoRepo.SaveChangesAsync();

            _logger.LogInformation("Imagen actualizada para equipo {Id}", equipo.Id);

            return ServiceResult.Ok(new
            {
                mensaje = "Imagen actualizada exitosamente",
                imagenUrl = equipo.ImagenUrl
            });

            */
            return ServiceResult.Ok("Aun no implementado");
        }

        public async Task<ServiceResult> DeleteEquipoAsync(int id, string webRootPath)
        {
            var equipo = await _equipoRepo.GetByIdAsync(id);
            if (equipo == null)
                return ServiceResult.BadRequest("Equipo no encontrado");

            if (!string.IsNullOrEmpty(equipo.ImagenUrl))
            {
                var imagePath = Path.Combine(webRootPath, equipo.ImagenUrl.TrimStart('/'));
                if (File.Exists(imagePath))
                    File.Delete(imagePath);
            }

            await _equipoRepo.DeleteAsync(equipo);
            _logger.LogInformation("Equipo eliminado {Id}", id);

            return ServiceResult.Ok("Equipo eliminado exitosamente");
        }
    }
}
