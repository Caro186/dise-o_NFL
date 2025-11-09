using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Logic.Interfaces;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Persistence.Models;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using NFLFantasyAPI.CrossCutting;

namespace NFLFantasyAPI.Logic.Services
{
    public class LigaService : ILigaService
    {
        private readonly ILigaRepository _ligaRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ITemporadaRepository _temporadaRepo;
        private readonly IEquipoRepository _equipoRepo;
        private readonly IEquipoLigaRepository _equipoLigaRepo;
        private readonly ILogger<LigaService> _logger;

        public LigaService(
            ILigaRepository ligaRepo,
            IUsuarioRepository usuarioRepo,
            ITemporadaRepository temporadaRepo,
            IEquipoRepository equipoRepo,
            IEquipoLigaRepository equipoLigaRepo,
            ILogger<LigaService> logger)
        {
            _ligaRepo = ligaRepo;
            _usuarioRepo = usuarioRepo;
            _temporadaRepo = temporadaRepo;
            _equipoRepo = equipoRepo;
            _equipoLigaRepo = equipoLigaRepo;
            _logger = logger;
        }

        public async Task<ServiceResult> CrearLigaAsync(LigaCreateDto dto)
        {
            try
            {
                // Validar cantidad de equipos
                int[] cantidadesValidas = { 4, 6, 8, 10, 12, 14, 16, 18, 20 };
                if (!cantidadesValidas.Contains(dto.CantidadEquipos))
                    return ServiceResult.BadRequest("Cantidad de equipos inválida");

                var temporada = await _temporadaRepo.GetActualesAsync();
                if (temporada == null)
                    return ServiceResult.BadRequest("No hay temporada activa");

                if (await _ligaRepo.ExistsByNameAsync(dto.NombreLiga, temporada.Id))
                    return ServiceResult.BadRequest("Ya existe una liga con ese nombre");

                var usuario = await _usuarioRepo.GetByIdAsync(dto.IdComisionado);
                if (usuario == null)
                    return ServiceResult.BadRequest("Usuario no encontrado");

                string configPlayoffs = dto.EquiposEnPlayoffs == 6
                    ? "{\"equipos\":6,\"semanas\":[16,17,18]}"
                    : "{\"equipos\":4,\"semanas\":[16,17]}";

                var liga = new Liga
                {
                    NombreLiga = dto.NombreLiga,
                    Descripcion = dto.Descripcion,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    IdTemporada = temporada.Id,
                    Estado = "Pre-Draft",
                    CuposTotales = dto.CantidadEquipos,
                    CuposOcupados = 1,
                    FechaCreacion = DateTime.UtcNow,
                    IdComisionado = dto.IdComisionado,
                    ConfigPlayoffs = configPlayoffs,
                    PermitirDecimales = true
                };

                await _ligaRepo.AddAsync(liga);
                await _ligaRepo.SaveChangesAsync();

                // Crear equipo comisionado
                var equipo = new Equipo
                {
                    Nombre = dto.NombreEquipoComisionado,
                    UsuarioId = dto.IdComisionado,
                    Liga = liga.NombreLiga,
                    Estado = "Activo",
                    FechaCreacion = DateTime.UtcNow
                };
                await _equipoRepo.AddAsync(equipo);
                await _equipoRepo.SaveChangesAsync();

                var equipoLiga = new EquipoLiga
                {
                    IdEquipo = equipo.Id,
                    IdLiga = liga.IdLiga,
                    Alias = usuario.NombreCompleto,
                    FechaUnion = DateTime.UtcNow,
                    EsComisionado = true
                };

                await _equipoLigaRepo.AddAsync(equipoLiga);
                await _equipoLigaRepo.SaveChangesAsync();

                _logger.LogInformation("Liga creada correctamente: {Liga}", liga.NombreLiga);

                var result = new
                {
                    liga.IdLiga,
                    liga.NombreLiga,
                    liga.Estado,
                    liga.CuposTotales,
                    liga.CuposOcupados,
                    Comisionado = usuario.NombreCompleto
                };

                return ServiceResult.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear liga");
                return ServiceResult.Error("Error interno del servidor al crear liga");
            }
        }

        public async Task<ServiceResult> UnirseALigaAsync(UnirseALigaDto dto)
        {
            try
            {
                var liga = await _ligaRepo.GetByIdAsync(dto.IdLiga);
                if (liga == null)
                    return ServiceResult.BadRequest("Liga no encontrada");

                if (liga.CuposOcupados >= liga.CuposTotales)
                    return ServiceResult.BadRequest("No hay cupos disponibles");

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, liga.PasswordHash))
                    return ServiceResult.BadRequest("Contraseña incorrecta");

                var usuario = await _usuarioRepo.GetByIdAsync(dto.IdUsuario);
                if (usuario == null)
                    return ServiceResult.BadRequest("Usuario no encontrado");

                // Crear equipo
                var equipo = new Equipo
                {
                    Nombre = dto.NombreEquipo,
                    UsuarioId = dto.IdUsuario,
                    Liga = liga.NombreLiga,
                    Estado = "Activo",
                    FechaCreacion = DateTime.UtcNow
                };
                await _equipoRepo.AddAsync(equipo);
                await _equipoRepo.SaveChangesAsync();

                var equipoLiga = new EquipoLiga
                {
                    IdEquipo = equipo.Id,
                    IdLiga = liga.IdLiga,
                    Alias = dto.Alias,
                    FechaUnion = DateTime.UtcNow,
                    EsComisionado = false
                };

                await _equipoLigaRepo.AddAsync(equipoLiga);
                await _equipoLigaRepo.SaveChangesAsync();
                ;
                liga.CuposOcupados++;
                await _ligaRepo.SaveChangesAsync();

                var result = new
                {
                    mensaje = "Unido exitosamente",
                    liga = liga.NombreLiga,
                    equipo = equipo.Nombre
                };

                return ServiceResult.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unirse a liga");
                return ServiceResult.Error("Error interno del servidor al unirse a liga");
            }
        }

        public async Task<ServiceResult> ObtenerLigaAsync(int id)
        {
            var liga = await _ligaRepo.GetByIdAsync(id);
            if (liga == null)
                return ServiceResult.BadRequest("Liga no encontrada");

            return ServiceResult.Ok(liga);
        }

        public async Task<ServiceResult> ObtenerTodasAsync()
        {
            var ligas = await _ligaRepo.GetAllAsync();
            return ServiceResult.Ok(ligas);
        }
    }
}
