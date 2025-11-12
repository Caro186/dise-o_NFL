using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.CrossCutting.Configuration;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using BCrypt.Net;

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


        public LigaService(
            ILigaRepository ligaRepository,
            IUsuarioRepository usuarioRespository,
            IEquipoFantasyRepository equipoFantasyRepository,
            IWebHostEnvironment environment,
            ILogger<LigaService> logger,
            IOptions<FileServerSettings> fileServerSettings)
        {
            _ligaRepository = ligaRepository;
            _environment = environment;
            _logger = logger;
            _fileServerSettings = fileServerSettings.Value;
            _usuarioRespository = usuarioRespository;
            _equipoFantasyRepository = equipoFantasyRepository;
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
            var liga = await _ligaRepository.GetByIdAsync(id);
            if (liga == null)
                return ServiceResult.BadRequest("Liga no encontrada");

            return ServiceResult.Ok(liga);
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

        public async Task<ServiceResult> UnirseLigaAsync(UnirseLigaDto dto)
        {
            // 1. Validar existencia de liga
            var liga = await _ligaRepository.GetByIdAsync(dto.LigaId);
            if (liga == null)
                return ServiceResult.BadRequest("Liga no encontrada");

            // 2. Verificar contraseña
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, liga.PasswordHash))
                return ServiceResult.BadRequest("Contraseña incorrecta");

            // 3. Cupos disponibles
            if (liga.CuposOcupados >= liga.CuposTotales)
                return ServiceResult.BadRequest("La liga está llena");

            // 4. Validar usuario
            var usuario = await _usuarioRespository.GetByIdAsync(dto.UsuarioId);
            if (usuario == null)
                return ServiceResult.BadRequest("Usuario no encontrado");

            // 5. Validar equipo
            var equipoFantasy = await _equipoFantasyRepository.GetByIdAsync(dto.EquipoId);
            if (equipoFantasy == null)
                return ServiceResult.BadRequest("Equipo fantasy no encontrado");

            if (equipoFantasy.UsuarioId != dto.UsuarioId)
                return ServiceResult.BadRequest("El equipo no pertenece al usuario");

            if (equipoFantasy.LigaId.HasValue)
                return ServiceResult.BadRequest("El equipo ya está en otra liga");

            // 6. Verificar si ya está en liga
            var equipos = await _equipoFantasyRepository.GetByUsuarioIdAsync(dto.UsuarioId);
            if (equipos != null)
            {
                foreach (var equipo in equipos)
                {
                    if (equipo.LigaId == dto.LigaId)
                    {
                        return ServiceResult.BadRequest("Ya tienes un equipo en esta liga");
                    }
                }
            }

            // 7. Actualizar entidades
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

    }
}
