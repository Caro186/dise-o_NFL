using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NFLFantasyAPI.Persistence.Models
{
    /// <summary>
    /// Representa una liga de Fantasy NFL
    /// </summary>
    public class Liga
    {
        /// <summary>
        /// Identificador único de la liga
        /// </summary>
        [Key]
        public int IdLiga { get; set; }

        /// <summary>
        /// URL de la imagen de la liga
        /// </summary>
        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        /// <summary>
        /// Nombre de la liga (1-100 caracteres según user story)
        /// </summary>
        [Required(ErrorMessage = "El nombre de la liga es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string NombreLiga { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional de la liga
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Contraseña hasheada de la liga (8-12 caracteres, alfanumérica, min 1 mayúscula y 1 minúscula)
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// ID de la temporada actual
        /// </summary>
        [Required(ErrorMessage = "La temporada es obligatoria")]
        public int IdTemporada { get; set; }

        /// <summary>
        /// Estado de la liga (Pre-Draft por defecto)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pre-Draft";

        /// <summary>
        /// Cantidad total de equipos permitidos (4, 6, 8, 10, 12, 14, 16, 18 o 20)
        /// </summary>
        [Required(ErrorMessage = "Los cupos totales son obligatorios")]
        public int CuposTotales { get; set; }

        /// <summary>
        /// Cantidad de cupos ocupados actualmente
        /// </summary>
        public int CuposOcupados { get; set; } = 1;

        /// <summary>
        /// Fecha de creación de la liga (autogenerada)
        /// </summary>
        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de inicio de la liga
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Fecha de fin de la liga
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// ID del usuario comisionado principal (creador)
        /// </summary>
        [Required(ErrorMessage = "El comisionado es obligatorio")]
        public int IdComisionado { get; set; }

        /// <summary>
        /// Formato de posiciones por defecto (JSON serializado)
        /// </summary>
        [Required]
        public string FormatoPosiciones { get; set; } = "{\"QB\":1,\"RB\":2,\"K\":1,\"DEF\":1,\"WR\":2,\"RB_WR\":1,\"TE\":1,\"BENCH\":6,\"IR\":3}";

        /// <summary>
        /// Esquema de puntuación por defecto (JSON serializado)
        /// </summary>
        [Required]
        public string EsquemaPuntos { get; set; } = "{\"PassingYards\":0.04,\"PassingTDs\":4,\"Interceptions\":-2,\"RushingYards\":0.1,\"Receptions\":1,\"ReceivingYards\":0.1,\"RushRecvTDs\":6,\"Sacks\":1,\"InterceptionsDefense\":2,\"FumblesRecovered\":2,\"Safeties\":2,\"DefensiveTDs\":6,\"Def2ptReturn\":2,\"PATMade\":1,\"FGMade0to50\":3,\"FGMade50plus\":5,\"PointsAllowed10orLess\":5,\"PointsAllowed11to20\":2,\"PointsAllowed21to30\":0,\"PointsAllowedOver30\":-2}";

        /// <summary>
        /// Configuración de playoffs (JSON serializado)
        /// </summary>
        [Required]
        public string ConfigPlayoffs { get; set; } = "{\"equipos\":4,\"semanas\":[16,17]}";

        /// <summary>
        /// Fecha límite de intercambios (inactiva por defecto)
        /// </summary>
        public DateTime? FechaLimiteIntercambios { get; set; }

        /// <summary>
        /// Límite máximo de cambios por temporada por equipo (null = sin límite)
        /// </summary>
        public int? LimiteMaximoCambios { get; set; }

        /// <summary>
        /// Límite máximo de contrataciones de agentes libres por temporada por equipo (null = sin límite)
        /// </summary>
        public int? LimiteMaximoContrataciones { get; set; }

        /// <summary>
        /// Indica si se permiten puntajes con decimales
        /// </summary>
        [Required]
        public bool PermitirDecimales { get; set; } = true;

        /// <summary>
        /// Propiedad de navegación al comisionado
        /// </summary>
        [ForeignKey("IdComisionado")]
        public virtual Usuario? Comisionado { get; set; }

        /// <summary>
        /// Propiedad de navegación a la temporada
        /// </summary>
        [ForeignKey("IdTemporada")]
        public virtual Temporada? Temporada { get; set; }
    }
}
