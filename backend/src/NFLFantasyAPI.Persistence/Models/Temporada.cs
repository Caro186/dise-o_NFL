using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFLFantasyAPI.Persistence.Models
{
    public class Temporada
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // Identificador único autogenerado

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaCierre { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;  // Fecha de creación autogenerada

        public bool Actual { get; set; } = false;  // Solo una temporada puede ser actual

        // Relación con semanas
        public ICollection<Semana> Semanas { get; set; } = new List<Semana>();
    }
}
