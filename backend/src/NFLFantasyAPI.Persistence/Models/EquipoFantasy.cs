using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Persistence.Models
{
    /// <summary>
    /// Representa un equipo de fantasy creado por un usuario
    /// </summary>
    public class EquipoFantasy
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int UsuarioId { get; set; }

        public int? LigaId { get; set; }

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Estado { get; set; } = "Activo";

        // Propiedades de navegación
        public Usuario? Usuario { get; set; }
        public Liga? Liga { get; set; }
    }
}
