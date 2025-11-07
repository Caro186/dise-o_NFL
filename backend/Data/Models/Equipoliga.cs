using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Models
{
    /// <summary>
    /// Representa la relación entre un equipo y una liga
    /// Permite que un usuario tenga múltiples equipos en diferentes ligas
    /// </summary>
    public class EquipoLiga
    {
        /// <summary>
        /// Identificador único de la relación
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID del equipo
        /// </summary>
        [Required]
        public int IdEquipo { get; set; }

        /// <summary>
        /// ID de la liga
        /// </summary>
        [Required]
        public int IdLiga { get; set; }

        /// <summary>
        /// Alias del usuario en esta liga específica
        /// </summary>
        [Required(ErrorMessage = "El alias es obligatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El alias debe tener entre 1 y 50 caracteres")]
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// Fecha en que el equipo se unió a la liga
        /// </summary>
        [Required]
        public DateTime FechaUnion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si el usuario es comisionado de esta liga
        /// </summary>
        [Required]
        public bool EsComisionado { get; set; } = false;

        /// <summary>
        /// Navegación al equipo
        /// </summary>
        public virtual Equipo? Equipo { get; set; }

        /// <summary>
        /// Navegación a la liga
        /// </summary>
        public virtual Liga? Liga { get; set; }
    }
}