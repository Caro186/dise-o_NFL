using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFLFantasyAPI.Persistence.Models
{
    public class Semana
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // Identificador único autogenerado

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        // Relación con Temporada
        [Required]
        public int TemporadaId { get; set; }

        [Required]
        [ForeignKey("TemporadaId")]
        public Temporada Temporada { get; set; } = null!;
    }
}

