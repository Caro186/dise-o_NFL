using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Persistence.Models
{
    public class Semana
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public int TemporadaId { get; set; }

        // Propiedad de navegación (SIN [ForeignKey] aquí, se configura en DbContext)
        public Temporada? Temporada { get; set; }
    }
}
