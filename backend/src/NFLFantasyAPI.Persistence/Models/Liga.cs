using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFLFantasyAPI.Persistence.Models
{
    public class Liga
    {
        [Key]
        public int IdLiga { get; set; }

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string NombreLiga { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int IdTemporada { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pre-Draft";

        [Required]
        public int CuposTotales { get; set; }

        public int CuposOcupados { get; set; } = 1;

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        [Required]
        public int ComisionadoId { get; set; }

        public string FormatoPosiciones { get; set; } = string.Empty;
        public string EsquemaPuntos { get; set; } = string.Empty;
        public string ConfigPlayoffs { get; set; } = string.Empty;
        public bool PermitirDecimales { get; set; } = true;

        // Propiedades de navegación (SIN [ForeignKey] aquí, se configura en DbContext)
        public Usuario? Comisionado { get; set; }
        public Temporada? Temporada { get; set; }
    }
}
