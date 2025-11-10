using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para crear una liga
    /// </summary>
    public class LigaCreateDto
    {
        [Required(ErrorMessage = "El nombre de la liga es obligatorio")]
        [MaxLength(100)]
        public string NombreLiga { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int IdTemporada { get; set; }

        [Required]
        [Range(2, 20, ErrorMessage = "Los cupos deben estar entre 2 y 20")]
        public int CuposTotales { get; set; }

        [Required]
        public int ComisionadoId { get; set; }

        public string FormatoPosiciones { get; set; } = string.Empty;
        public string EsquemaPuntos { get; set; } = string.Empty;
        public string ConfigPlayoffs { get; set; } = string.Empty;
        public bool PermitirDecimales { get; set; } = true;

        public int? EquipoFantasyId { get; set; }
    }
    /// <summary>
    /// DTO de respuesta de liga
    /// </summary>
    public class LigaResponseDto
    {
        public int IdLiga { get; set; }
        public string? ImagenUrl { get; set; }
        public string NombreLiga { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int IdTemporada { get; set; }
        public string? NombreTemporada { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int CuposTotales { get; set; }
        public int CuposOcupados { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int ComisionadoId { get; set; }
        public string? NombreComisionado { get; set; }
        public string FormatoPosiciones { get; set; } = string.Empty;
        public string EsquemaPuntos { get; set; } = string.Empty;
        public string ConfigPlayoffs { get; set; } = string.Empty;
        public bool PermitirDecimales { get; set; }
    }


    public class UnirseLigaDto
    {
        [Required]
        public int LigaId { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int EquipoId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Alias { get; set; } = string.Empty;
    }
}
