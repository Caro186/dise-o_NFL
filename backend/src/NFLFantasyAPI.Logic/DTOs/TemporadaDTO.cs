using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para crear una semana
    /// </summary>
    public class CrearSemanaDto
    {
        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime FechaFin { get; set; }
    }

    /// <summary>
    /// DTO para crear una temporada
    /// </summary>
    public class CrearTemporadaDto
    {
        [Required(ErrorMessage = "El nombre de la temporada es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de cierre es obligatoria")]
        public DateTime FechaCierre { get; set; }

        [Required]
        public bool Actual { get; set; } = false;

        public List<CrearSemanaDto>? Semanas { get; set; }
    }

    /// <summary>
    /// DTO de respuesta de temporada
    /// </summary>
    public class TemporadaResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCierre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Actual { get; set; }
    }
}

