using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using BCrypt.Net;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LigasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public LigasController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Ligas/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeagueById(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"SELECT id_liga, nombre_liga, descripcion, temporada, estado, 
                                    cupos_totales, cupos_ocupados, fecha_creacion, fecha_inicio, fecha_fin
                                    FROM ligas WHERE id_liga = @id";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var liga = new
                                {
                                    id_liga = reader.GetInt32("id_liga"),
                                    nombre_liga = reader.GetString("nombre_liga"),
                                    descripcion = reader.IsDBNull("descripcion") ? "" : reader.GetString("descripcion"),
                                    temporada = reader.GetString("temporada"),
                                    estado = reader.GetString("estado"),
                                    cupos_totales = reader.GetInt32("cupos_totales"),
                                    cupos_ocupados = reader.GetInt32("cupos_ocupados"),
                                    fecha_creacion = reader.GetDateTime("fecha_creacion"),
                                    fecha_inicio = reader.IsDBNull("fecha_inicio") ? (DateTime?)null : reader.GetDateTime("fecha_inicio"),
                                    fecha_fin = reader.IsDBNull("fecha_fin") ? (DateTime?)null : reader.GetDateTime("fecha_fin")
                                };
                                
                                return Ok(liga);
                            }
                            
                            return NotFound(new { message = "Liga no encontrada" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la liga", error = ex.Message });
            }
        }

        // GET: api/Ligas/search?nombre=&temporada=&estado=
        [HttpGet("search")]
        public async Task<IActionResult> SearchLeagues([FromQuery] string? nombre, [FromQuery] string? temporada, [FromQuery] string? estado)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"SELECT id_liga, nombre_liga, descripcion, temporada, estado, 
                                    cupos_totales, cupos_ocupados, fecha_creacion
                                    FROM ligas WHERE 1=1";
                    
                    var parameters = new List<MySqlParameter>();
                    
                    if (!string.IsNullOrEmpty(nombre))
                    {
                        query += " AND nombre_liga LIKE @nombre";
                        parameters.Add(new MySqlParameter("@nombre", $"%{nombre}%"));
                    }
                    
                    if (!string.IsNullOrEmpty(temporada))
                    {
                        query += " AND temporada = @temporada";
                        parameters.Add(new MySqlParameter("@temporada", temporada));
                    }
                    
                    if (!string.IsNullOrEmpty(estado))
                    {
                        query += " AND estado = @estado";
                        parameters.Add(new MySqlParameter("@estado", estado));
                    }
                    
                    query += " ORDER BY fecha_creacion DESC";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        
                        var ligas = new List<object>();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ligas.Add(new
                                {
                                    id_liga = reader.GetInt32("id_liga"),
                                    nombre_liga = reader.GetString("nombre_liga"),
                                    descripcion = reader.IsDBNull("descripcion") ? "" : reader.GetString("descripcion"),
                                    temporada = reader.GetString("temporada"),
                                    estado = reader.GetString("estado"),
                                    cupos_totales = reader.GetInt32("cupos_totales"),
                                    cupos_ocupados = reader.GetInt32("cupos_ocupados"),
                                    fecha_creacion = reader.GetDateTime("fecha_creacion")
                                });
                            }
                        }
                        
                        return Ok(ligas);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar ligas", error = ex.Message });
            }
        }

        // GET: api/Ligas/5/users/1
        [HttpGet("{idLiga}/users/{idUsuario}")]
        public async Task<IActionResult> CheckUserInLeague(int idLiga, int idUsuario)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"SELECT COUNT(*) as count 
                                    FROM participantes_liga 
                                    WHERE id_liga = @idLiga AND id_usuario = @idUsuario AND activo = 1";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idLiga", idLiga);
                        command.Parameters.AddWithValue("@idUsuario", idUsuario);
                        
                        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                        
                        return Ok(new { exists = count > 0 });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al verificar membresía", error = ex.Message });
            }
        }

        // POST: api/Ligas/join
        [HttpPost("join")]
        public async Task<IActionResult> JoinLeague([FromBody] JoinLeagueRequest request)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Validaciones
                        if (request == null || request.id_liga == 0 || string.IsNullOrEmpty(request.password) ||
                            string.IsNullOrEmpty(request.alias) || string.IsNullOrEmpty(request.nombre_equipo))
                        {
                            return BadRequest(new { success = false, message = "Todos los campos son requeridos" });
                        }

                        if (request.alias.Length > 50)
                        {
                            return BadRequest(new { success = false, message = "El alias debe tener máximo 50 caracteres" });
                        }

                        if (request.nombre_equipo.Length > 100)
                        {
                            return BadRequest(new { success = false, message = "El nombre del equipo debe tener máximo 100 caracteres" });
                        }

                        // 1. Verificar que la liga existe y obtener datos
                        string queryLiga = @"SELECT id_liga, password_hash, estado, cupos_totales, cupos_ocupados 
                                           FROM ligas WHERE id_liga = @id_liga";
                        
                        string? passwordHash = null;
                        string? estado = null;
                        int cuposTotales = 0;
                        int cuposOcupados = 0;
                        
                        using (var command = new MySqlCommand(queryLiga, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id_liga", request.id_liga);
                            
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (!await reader.ReadAsync())
                                {
                                    await transaction.RollbackAsync();
                                    return NotFound(new { success = false, message = "La liga no existe" });
                                }
                                
                                passwordHash = reader.GetString("password_hash");
                                estado = reader.GetString("estado");
                                cuposTotales = reader.GetInt32("cupos_totales");
                                cuposOcupados = reader.GetInt32("cupos_ocupados");
                            }
                        }

                        // 2. Verificar que la liga está activa
                        if (estado != "activa")
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { success = false, message = "La liga no está disponible en este momento" });
                        }

                        // 3. Verificar cupos disponibles
                        if (cuposOcupados >= cuposTotales)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { success = false, message = "La liga no tiene cupos disponibles" });
                        }

                        // 4. Verificar contraseña
                        bool passwordMatch = BCrypt.Net.BCrypt.Verify(request.password, passwordHash);
                        if (!passwordMatch)
                        {
                            await transaction.RollbackAsync();
                            return Unauthorized(new { success = false, message = "No se pudo unir a la liga. Verifica los datos e intenta nuevamente" });
                        }

                        // 5. Verificar que el usuario no está ya en la liga
                        string queryUserExists = @"SELECT COUNT(*) FROM participantes_liga 
                                                 WHERE id_liga = @id_liga AND id_usuario = @id_usuario AND activo = 1";
                        
                        using (var command = new MySqlCommand(queryUserExists, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id_liga", request.id_liga);
                            command.Parameters.AddWithValue("@id_usuario", request.id_usuario);
                            
                            int userExists = Convert.ToInt32(await command.ExecuteScalarAsync());
                            if (userExists > 0)
                            {
                                await transaction.RollbackAsync();
                                return BadRequest(new { success = false, message = "Ya eres miembro de esta liga" });
                            }
                        }

                        // 6. Verificar que el alias es único
                        string queryAliasExists = @"SELECT COUNT(*) FROM participantes_liga 
                                                  WHERE id_liga = @id_liga AND alias = @alias AND activo = 1";
                        
                        using (var command = new MySqlCommand(queryAliasExists, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id_liga", request.id_liga);
                            command.Parameters.AddWithValue("@alias", request.alias);
                            
                            int aliasExists = Convert.ToInt32(await command.ExecuteScalarAsync());
                            if (aliasExists > 0)
                            {
                                await transaction.RollbackAsync();
                                return BadRequest(new { success = false, message = "El alias ya está en uso. Por favor elige otro" });
                            }
                        }

                        // 7. Verificar que el nombre del equipo es único
                        string queryTeamExists = @"SELECT COUNT(*) FROM participantes_liga 
                                                 WHERE id_liga = @id_liga AND nombre_equipo = @nombre_equipo AND activo = 1";
                        
                        using (var command = new MySqlCommand(queryTeamExists, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id_liga", request.id_liga);
                            command.Parameters.AddWithValue("@nombre_equipo", request.nombre_equipo);
                            
                            int teamExists = Convert.ToInt32(await command.ExecuteScalarAsync());
                            if (teamExists > 0)
                            {
                                await transaction.RollbackAsync();
                                return BadRequest(new { success = false, message = "El nombre del equipo ya está en uso. Por favor elige otro" });
                            }
                        }

                        // 8. Insertar participante
                        string queryInsert = @"INSERT INTO participantes_liga 
                                             (id_liga, id_usuario, alias, nombre_equipo, fecha_incorporacion, activo) 
                                             VALUES (@id_liga, @id_usuario, @alias, @nombre_equipo, NOW(), 1)";
                        
                        long idParticipante;
                        using (var command = new MySqlCommand(queryInsert, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id_liga", request.id_liga);
                            command.Parameters.AddWithValue("@id_usuario", request.id_usuario);
                            command.Parameters.AddWithValue("@alias", request.alias);
                            command.Parameters.AddWithValue("@nombre_equipo", request.nombre_equipo);
                            
                            await command.ExecuteNonQueryAsync();
                            idParticipante = command.LastInsertedId;
                        }

                        // 9. Actualizar cupos ocupados
                        string queryUpdate = @"UPDATE ligas SET cupos_ocupados = cupos_ocupados + 1 WHERE id_liga = @id_liga";
                        
                        using (var command = new MySqlCommand(queryUpdate, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id_liga", request.id_liga);
                            await command.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();

                        return Ok(new
                        {
                            success = true,
                            message = "¡Te has unido exitosamente a la liga!",
                            id_participante = idParticipante
                        });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, new { success = false, message = "Error al procesar la solicitud", error = ex.Message });
                    }
                }
            }
        }
    }

    public class JoinLeagueRequest
    {
        public int id_liga { get; set; }
        public int id_usuario { get; set; }
        public string password { get; set; } = string.Empty;
        public string alias { get; set; } = string.Empty;
        public string nombre_equipo { get; set; } = string.Empty;
    }
}