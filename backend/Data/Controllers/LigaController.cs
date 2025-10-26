using Microsoft.AspNetCore.Mvc;
using NFLFantasyAPI.Data;
using NFLFantasyAPI.DTOs;
using NFLFantasyAPI.Models;
using System.Threading.Tasks;
using Npgsql;
using System;
using System.Collections.Generic;

namespace NFLFantasyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LigasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public LigasController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public async Task<IActionResult> CreateLiga([FromBody] LigaCreateDto ligaCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var liga = new Liga
            {
                ImagenUrl = ligaCreateDto.ImagenUrl,
                NombreLiga = ligaCreateDto.NombreLiga,
                Descripcion = ligaCreateDto.Descripcion,
                PasswordHash = ligaCreateDto.PasswordHash,
                Temporada = ligaCreateDto.Temporada,
                Estado = ligaCreateDto.Estado,
                CuposTotales = ligaCreateDto.CuposTotales,
                FechaInicio = ligaCreateDto.FechaInicio,
                FechaFin = ligaCreateDto.FechaFin,
                IdCreador = ligaCreateDto.IdCreador,
            };

            await _context.Ligas.AddAsync(liga);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLigaById), new { id = liga.IdLiga }, liga);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLigaById(int id)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"SELECT id_liga, nombre_liga, descripcion, temporada, estado, 
                                cupos_totales, cupos_ocupados, fecha_creacion, fecha_inicio, fecha_fin
                                FROM ligas WHERE id_liga = @id_liga";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("id_liga", id);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var liga = new
                    {
                        id_liga = reader.GetInt32(reader.GetOrdinal("id_liga")),
                        nombre_liga = reader.GetString(reader.GetOrdinal("nombre_liga")),
                        descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? "" : reader.GetString(reader.GetOrdinal("descripcion")),
                        temporada = reader.GetString(reader.GetOrdinal("temporada")),
                        estado = reader.GetString(reader.GetOrdinal("estado")),
                        cupos_totales = reader.GetInt32(reader.GetOrdinal("cupos_totales")),
                        cupos_ocupados = reader.GetInt32(reader.GetOrdinal("cupos_ocupados")),
                        fecha_creacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion")),
                        fecha_inicio = reader.IsDBNull(reader.GetOrdinal("fecha_inicio")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("fecha_inicio")),
                        fecha_fin = reader.IsDBNull(reader.GetOrdinal("fecha_fin")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("fecha_fin"))
                    };
                    return Ok(liga);
                }
                return NotFound(new { message = "Liga no encontrada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la liga", error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchLeagues([FromQuery] string? nombre, [FromQuery] string? temporada, [FromQuery] string? estado)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"SELECT id_liga, nombre_liga, descripcion, temporada, estado, 
                                cupos_totales, cupos_ocupados, fecha_creacion
                                FROM ligas WHERE 1=1";

                var parameters = new List<NpgsqlParameter>();

                if (!string.IsNullOrEmpty(nombre))
                {
                    query += " AND nombre_liga ILIKE @nombre";
                    parameters.Add(new NpgsqlParameter("nombre", $"%{nombre}%"));
                }

                if (!string.IsNullOrEmpty(temporada))
                {
                    query += " AND temporada = @temporada";
                    parameters.Add(new NpgsqlParameter("temporada", temporada));
                }

                if (!string.IsNullOrEmpty(estado))
                {
                    query += " AND estado = @estado";
                    parameters.Add(new NpgsqlParameter("estado", estado));
                }

                query += " ORDER BY fecha_creacion DESC";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());

                var ligas = new List<object>();

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ligas.Add(new
                    {
                        id_liga = reader.GetInt32(reader.GetOrdinal("id_liga")),
                        nombre_liga = reader.GetString(reader.GetOrdinal("nombre_liga")),
                        descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? "" : reader.GetString(reader.GetOrdinal("descripcion")),
                        temporada = reader.GetString(reader.GetOrdinal("temporada")),
                        estado = reader.GetString(reader.GetOrdinal("estado")),
                        cupos_totales = reader.GetInt32(reader.GetOrdinal("cupos_totales")),
                        cupos_ocupados = reader.GetInt32(reader.GetOrdinal("cupos_ocupados")),
                        fecha_creacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion"))
                    });
                }
                return Ok(ligas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar ligas", error = ex.Message });
            }
        }

        [HttpGet("{idLiga}/users/{idUsuario}")]
        public async Task<IActionResult> CheckUserInLeague(int idLiga, int idUsuario)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"SELECT COUNT(*) FROM participantes_liga 
                                 WHERE id_liga = @idLiga AND id_usuario = @idUsuario AND activo = TRUE";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("idLiga", idLiga);
                command.Parameters.AddWithValue("idUsuario", idUsuario);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());

                return Ok(new { exists = count > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al verificar membresía", error = ex.Message });
            }
        }
    }
}
