using NFLFantasyAPI.CrossCutting;
using NFLFantasyAPI.Logic.DTOs;
using NFLFantasyAPI.Persistence.Models;
using NFLFantasyAPI.Persistence.Interfaces;
using NFLFantasyAPI.Logic.Interfaces;

namespace NFLFantasyAPI.Logic.Services
{
    public class JugadorService : IJugadorService
    {
        private readonly IJugadorRepository _repository;

        public JugadorService(IJugadorRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServiceResult> GetAllAsync()
        {
            var jugadores = await _repository.GetAllAsync();

            var dto = jugadores.Select(j => new JugadorListDto
            {
                Id = j.Id,
                Nombre = j.Nombre,
                Posicion = j.Posicion,
                NombreEquipoNFL = j.EquipoNFL.Nombre,
                ThumbnailUrl = j.ThumbnailUrl,
                Estado = j.Estado
            }).ToList();

            return ServiceResult.Ok(dto);
        }

        public async Task<ServiceResult> GetByIdAsync(int id)
        {
            var jugador = await _repository.GetByIdAsync(id);
            if (jugador == null)
                return ServiceResult.BadRequest("Jugador no encontrado");

            var dto = new JugadorResponseDto
            {
                Id = jugador.Id,
                Nombre = jugador.Nombre,
                Posicion = jugador.Posicion,
                EquipoNFLId = jugador.EquipoNFLId,
                NombreEquipoNFL = jugador.EquipoNFL.Nombre,
                CiudadEquipoNFL = jugador.EquipoNFL.Ciudad,
                ImagenUrl = jugador.ImagenUrl,
                ThumbnailUrl = jugador.ThumbnailUrl,
                Estado = jugador.Estado,
                FechaCreacion = jugador.FechaCreacion,
                FechaActualizacion = jugador.FechaActualizacion
            };

            return ServiceResult.Ok(dto);
        }

        public async Task<ServiceResult> CreateAsync(CrearJugadorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Posicion) ||
                dto.EquipoNFLId <= 0)
                return ServiceResult.BadRequest("Todos los campos requeridos deben ser proporcionados");

            if (!await _repository.EquipoExistsAsync(dto.EquipoNFLId))
                return ServiceResult.BadRequest("El equipo NFL especificado no existe");

            if (await _repository.ExistsInEquipoAsync(dto.Nombre, dto.EquipoNFLId))
                return ServiceResult.BadRequest("Ya existe un jugador con ese nombre en el equipo especificado");

            var jugador = new Jugador
            {
                Nombre = dto.Nombre.Trim(),
                Posicion = dto.Posicion.Trim(),
                EquipoNFLId = dto.EquipoNFLId,
                ImagenUrl = dto.ImagenUrl?.Trim(),
                ThumbnailUrl = dto.ThumbnailUrl?.Trim(),
                Estado = "Activo",
                FechaCreacion = DateTime.UtcNow
            };

            await _repository.AddAsync(jugador);

            return ServiceResult.Ok(new { mensaje = "Jugador creado correctamente", jugador.Id });
        }

        public async Task<ServiceResult> UpdateAsync(int id, ActualizarJugadorDto dto)
        {
            var jugador = await _repository.GetByIdAsync(id);
            if (jugador == null)
                return ServiceResult.BadRequest("Jugador no encontrado");

            if (dto.EquipoNFLId.HasValue && !await _repository.EquipoExistsAsync(dto.EquipoNFLId.Value))
                return ServiceResult.BadRequest("El equipo NFL especificado no existe");

            if (!string.IsNullOrWhiteSpace(dto.Nombre))
            {
                var equipoId = dto.EquipoNFLId ?? jugador.EquipoNFLId;
                if (await _repository.ExistsInEquipoAsync(dto.Nombre, equipoId))
                    return ServiceResult.BadRequest("Ya existe un jugador con ese nombre en el equipo");
            }

            jugador.Nombre = dto.Nombre ?? jugador.Nombre;
            jugador.Posicion = dto.Posicion ?? jugador.Posicion;
            jugador.EquipoNFLId = dto.EquipoNFLId ?? jugador.EquipoNFLId;
            jugador.ImagenUrl = dto.ImagenUrl ?? jugador.ImagenUrl;
            jugador.ThumbnailUrl = dto.ThumbnailUrl ?? jugador.ThumbnailUrl;
            jugador.Estado = dto.Estado ?? jugador.Estado;
            jugador.FechaActualizacion = DateTime.UtcNow;

            await _repository.UpdateAsync(jugador);

            return ServiceResult.Ok(new { mensaje = "Jugador actualizado correctamente" });
        }

        public async Task<ServiceResult> DeleteAsync(int id, bool permanente)
        {
            var jugador = await _repository.GetByIdAsync(id);
            if (jugador == null)
                return ServiceResult.BadRequest("Jugador no encontrado");

            if (permanente)
            {
                await _repository.DeleteAsync(jugador);
                return ServiceResult.Ok(new { mensaje = "Jugador eliminado permanentemente" });
            }

            jugador.Estado = "Inactivo";
            jugador.FechaActualizacion = DateTime.UtcNow;
            await _repository.UpdateAsync(jugador);
            return ServiceResult.Ok(new { mensaje = "Jugador desactivado correctamente" });
        }

        public async Task<ServiceResult> GetByEquipoAsync(int equipoId)
        {
            if (!await _repository.EquipoExistsAsync(equipoId))
                return ServiceResult.BadRequest("Equipo NFL no encontrado");

            var jugadores = await _repository.GetByEquipoAsync(equipoId);

            var dto = jugadores.Select(j => new JugadorListDto
            {
                Id = j.Id,
                Nombre = j.Nombre,
                Posicion = j.Posicion,
                NombreEquipoNFL = j.EquipoNFL.Nombre,
                ThumbnailUrl = j.ThumbnailUrl,
                Estado = j.Estado
            }).ToList();

            return ServiceResult.Ok(dto);
        }

        public async Task<ServiceResult> GetByPosicionAsync(string posicion)
        {
            var jugadores = await _repository.GetByPosicionAsync(posicion);
            var dto = jugadores.Select(j => new JugadorListDto
            {
                Id = j.Id,
                Nombre = j.Nombre,
                Posicion = j.Posicion,
                NombreEquipoNFL = j.EquipoNFL.Nombre,
                ThumbnailUrl = j.ThumbnailUrl,
                Estado = j.Estado
            }).ToList();

            return ServiceResult.Ok(dto);
        }
    }
}
