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

        [Range(1, 100)]
        [Required]
        public int Nombre { get; set; }  // Número de 1 a 100

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
