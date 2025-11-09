using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para crear un equipo
    /// </summary>
    public class CrearEquipoDto
    {
        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int UsuarioId { get; set; }

        public string? ImagenUrl { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un equipo
    /// </summary>
    public class ActualizarEquipoDto
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int UsuarioId { get; set; }

        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string? Nombre { get; set; }

        public string? ImagenUrl { get; set; }
    }
}
