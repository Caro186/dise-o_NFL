using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    public class SemanaDTO
    {
        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime FechaFin { get; set; }
    }
}
