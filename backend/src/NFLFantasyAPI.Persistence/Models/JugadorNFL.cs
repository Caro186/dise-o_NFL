using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFLFantasyAPI.Persistence.Models
{
    public class Jugador
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(50)]
        public string Posicion { get; set; }

        [Required]
        public int EquipoNFLId { get; set; }

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Activo";

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        [ForeignKey("EquipoNFLId")]
        public virtual EquipoNFL EquipoNFL { get; set; }
    }
}
