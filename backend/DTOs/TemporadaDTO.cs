using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.DTOs
{
    public class TemporadaDTO
    {
        [Range(1, 100, ErrorMessage = "El nombre debe estar entre 1 y 100")]
        public int Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de cierre es obligatoria")]
        public DateTime FechaCierre { get; set; }

        public bool Actual { get; set; } = false;

        public List<SemanaDTO> Semanas { get; set; } = new List<SemanaDTO>();
    }
}
