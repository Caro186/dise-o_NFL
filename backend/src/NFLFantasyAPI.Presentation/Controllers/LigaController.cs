using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.Models;
using NFLFantasyAPI.DTOs;
using BCrypt.Net;

namespace NFLFantasyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LigaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LigaController> _logger;

        public LigaController(ApplicationDbContext context, ILogger<LigaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<LigaResponseDto>> CrearLiga(LigaCreateDto ligaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos de liga inválidos",
                        Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                int[] cantidadesValidas = { 4, 6, 8, 10, 12, 14, 16, 18, 20 };
                if (!cantidadesValidas.Contains(ligaDto.CantidadEquipos))
                {
                    return BadRequest(new ErrorResponseDto { Mensaje = "La cantidad de equipos debe ser 4, 6, 8, 10, 12, 14, 16, 18 o 20" });
                }

                var temporadaActual = await _context.Temporadas.FirstOrDefaultAsync(t => t.Actual);
                if (temporadaActual == null)
                {
                    return BadRequest(new ErrorResponseDto { Mensaje = "No hay una temporada activa" });
                }

                var ligaExistente = await _context.Ligas.AnyAsync(l => l.NombreLiga == ligaDto.NombreLiga && l.IdTemporada == temporadaActual.Id);
                if (ligaExistente)
                {
                    return BadRequest(new ErrorResponseDto { Mensaje = "Ya existe una liga con ese nombre" });
                }

                var usuario = await _context.Usuarios.FindAsync(ligaDto.IdComisionado);
                if (usuario == null)
                {
                    return NotFound(new ErrorResponseDto { Mensaje = "Usuario no encontrado" });
                }

                string configPlayoffs = ligaDto.EquiposEnPlayoffs == 6
                    ? "{\"equipos\":6,\"semanas\":[16,17,18]}"
                    : "{\"equipos\":4,\"semanas\":[16,17]}";

                var liga = new Liga
                {
                    NombreLiga = ligaDto.NombreLiga,
                    Descripcion = ligaDto.Descripcion,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(ligaDto.Password),
                    IdTemporada = temporadaActual.Id,
                    Estado = "Pre-Draft",
                    CuposTotales = ligaDto.CantidadEquipos,
                    CuposOcupados = 1,
                    FechaCreacion = DateTime.UtcNow,
                    IdComisionado = ligaDto.IdComisionado,
                    ConfigPlayoffs = configPlayoffs,
                    PermitirDecimales = true
                };

                _context.Ligas.Add(liga);
                await _context.SaveChangesAsync();

                var equipoComisionado = new Equipo
                {
                    Nombre = ligaDto.NombreEquipoComisionado,
                    UsuarioId = ligaDto.IdComisionado,
                    Liga = liga.NombreLiga,
                    Estado = "Activo",
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Equipos.Add(equipoComisionado);
                await _context.SaveChangesAsync();

                var equipoLiga = new EquipoLiga
                {
                    IdEquipo = equipoComisionado.Id,
                    IdLiga = liga.IdLiga,
                    Alias = usuario.NombreCompleto,
                    FechaUnion = DateTime.UtcNow,
                    EsComisionado = true
                };

                _context.EquiposLigas.Add(equipoLiga);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Liga creada: {Nombre}", liga.NombreLiga);

                var response = new LigaResponseDto
                {
                    IdLiga = liga.IdLiga,
                    NombreLiga = liga.NombreLiga,
                    Descripcion = liga.Descripcion,
                    IdTemporada = liga.IdTemporada,
                    Estado = liga.Estado,
                    CuposTotales = liga.CuposTotales,
                    CuposOcupados = liga.CuposOcupados,
                    CuposDisponibles = liga.CuposTotales - liga.CuposOcupados,
                    FechaCreacion = liga.FechaCreacion,
                    IdComisionado = liga.IdComisionado,
                    NombreComisionado = usuario.NombreCompleto,
                    IdEquipoComisionado = equipoComisionado.Id,
                    NombreEquipoComisionado = equipoComisionado.Nombre
                };

                return CreatedAtAction(nameof(ObtenerLiga), new { id = liga.IdLiga }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear liga");
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error al crear liga" });
            }
        }

        [HttpPost("unirse")]
        public async Task<ActionResult> UnirseALiga([FromBody] UnirseALigaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Mensaje = "Datos inválidos",
                        Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var liga = await _context.Ligas.FindAsync(dto.IdLiga);
                if (liga == null)
                {
                    return NotFound(new ErrorResponseDto { Mensaje = "Liga no encontrada" });
                }

                if (liga.Estado == "Finalizada")
                {
                    return BadRequest(new ErrorResponseDto { Mensaje = "Esta liga ya finalizó" });
                }

                if (liga.CuposOcupados >= liga.CuposTotales)
                {
                    return BadRequest(new ErrorResponseDto { Mensaje = "No hay cupos disponibles en esta liga" });
                }

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, liga.PasswordHash))
                {
                    _logger.LogWarning("Intento de unirse a liga con contraseña incorrecta: Liga {IdLiga}, Usuario {IdUsuario}", dto.IdLiga, dto.IdUsuario);
                    return StatusCode(403, new ErrorResponseDto { Mensaje = "Contraseña incorrecta" });
                }

                var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
                if (usuario == null)
                {
                    return NotFound(new ErrorResponseDto { Mensaje = "Usuario no encontrado" });
                }

                var yaEstaEnLiga = await _context.EquiposLigas
                    .AnyAsync(el => el.IdLiga == dto.IdLiga && 
                                    el.Equipo != null && 
                                    el.Equipo.UsuarioId == dto.IdUsuario);

                if (yaEstaEnLiga)
                {
                    return BadRequest(new ErrorResponseDto { Mensaje = "Ya perteneces a esta liga" });
                }

                var aliasExiste = await _context.EquiposLigas
                    .AnyAsync(el => el.IdLiga == dto.IdLiga && el.Alias == dto.Alias);

                if (aliasExiste)
                {
                    return BadRequest(new ErrorResponseDto { Mensaje = "El alias ya existe en esta liga. Por favor elige otro" });
                }

                var nombreEquipoExiste = await _context.EquiposLigas
                    .Include(el => el.Equipo)
                    .AnyAsync(el => el.IdLiga == dto.IdLiga && 
                                    el.Equipo != null && 
                                    el.Equipo.Nombre == dto.NombreEquipo);

                if (nombreEquipoExiste)
                {
                    return BadRequest(new ErrorResponseDto { Mensaje = "El nombre del equipo ya existe en esta liga. Por favor elige otro" });
                }

                var nuevoEquipo = new Equipo
                {
                    Nombre = dto.NombreEquipo,
                    UsuarioId = dto.IdUsuario,
                    Liga = liga.NombreLiga,
                    Estado = "Activo",
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Equipos.Add(nuevoEquipo);
                await _context.SaveChangesAsync();

                var equipoLiga = new EquipoLiga
                {
                    IdEquipo = nuevoEquipo.Id,
                    IdLiga = liga.IdLiga,
                    Alias = dto.Alias,
                    FechaUnion = DateTime.UtcNow,
                    EsComisionado = false
                };

                _context.EquiposLigas.Add(equipoLiga);

                liga.CuposOcupados++;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario {IdUsuario} se unió a liga {IdLiga} con equipo {NombreEquipo}", 
                    dto.IdUsuario, dto.IdLiga, dto.NombreEquipo);

                return Ok(new
                {
                    mensaje = "Te has unido exitosamente a la liga",
                    idLiga = liga.IdLiga,
                    nombreLiga = liga.NombreLiga,
                    idEquipo = nuevoEquipo.Id,
                    nombreEquipo = nuevoEquipo.Nombre,
                    cuposDisponibles = liga.CuposTotales - liga.CuposOcupados
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unirse a liga");
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error al unirse a la liga" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LigaResponseDto>> ObtenerLiga(int id)
        {
            try
            {
                var liga = await _context.Ligas.FindAsync(id);
                if (liga == null)
                {
                    return NotFound(new ErrorResponseDto { Mensaje = "Liga no encontrada" });
                }

                var comisionado = await _context.Usuarios.FindAsync(liga.IdComisionado);
                var equipoComisionado = await _context.Equipos.FirstOrDefaultAsync(e => e.UsuarioId == liga.IdComisionado && e.Liga == liga.NombreLiga);

                var response = new LigaResponseDto
                {
                    IdLiga = liga.IdLiga,
                    NombreLiga = liga.NombreLiga,
                    Descripcion = liga.Descripcion,
                    IdTemporada = liga.IdTemporada,
                    Estado = liga.Estado,
                    CuposTotales = liga.CuposTotales,
                    CuposOcupados = liga.CuposOcupados,
                    CuposDisponibles = liga.CuposTotales - liga.CuposOcupados,
                    FechaCreacion = liga.FechaCreacion,
                    IdComisionado = liga.IdComisionado,
                    NombreComisionado = comisionado?.NombreCompleto ?? "",
                    IdEquipoComisionado = equipoComisionado?.Id ?? 0,
                    NombreEquipoComisionado = equipoComisionado?.Nombre ?? ""
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener liga");
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error al obtener liga" });
            }
        }



        

        [HttpGet]
        public async Task<ActionResult<List<LigaResponseDto>>> ObtenerTodasLasLigas()
        {
            try
            {
                var ligas = await _context.Ligas.ToListAsync();
                
                var response = new List<LigaResponseDto>();
                foreach (var liga in ligas)
                {
                    var comisionado = await _context.Usuarios.FindAsync(liga.IdComisionado);
                    response.Add(new LigaResponseDto
                    {
                        IdLiga = liga.IdLiga,
                        NombreLiga = liga.NombreLiga,
                        Descripcion = liga.Descripcion,
                        IdTemporada = liga.IdTemporada,
                        Estado = liga.Estado,
                        CuposTotales = liga.CuposTotales,
                        CuposOcupados = liga.CuposOcupados,
                        CuposDisponibles = liga.CuposTotales - liga.CuposOcupados,
                        FechaCreacion = liga.FechaCreacion,
                        IdComisionado = liga.IdComisionado,
                        NombreComisionado = comisionado?.NombreCompleto ?? ""
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ligas");
                return StatusCode(500, new ErrorResponseDto { Mensaje = "Error al obtener ligas" });
            }
        }
    }
}