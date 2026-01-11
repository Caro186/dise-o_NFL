using System;
using System.ComponentModel.DataAnnotations;

namespace NFLFantasyAPI.Logic.DTOs
{
    /// <summary>
    /// DTO para la creación de una nueva liga
    /// </summary>
    public class CrearLigaDto
    {
        /// <summary>
        /// Nombre de la liga (1-100 caracteres)
        /// </summary>
        [Required(ErrorMessage = "El nombre de la liga es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string NombreLiga { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional de la liga
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Contraseña de la liga (8-12 caracteres, alfanumérica, min 1 mayúscula y 1 minúscula)
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(12, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 12 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z0-9]{8,12}$",
            ErrorMessage = "La contraseña debe ser alfanumérica con al menos una mayúscula y una minúscula")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad de equipos permitidos (4, 6, 8, 10, 12, 14, 16, 18 o 20)
        /// </summary>
        [Required(ErrorMessage = "La cantidad de equipos es obligatoria")]
        [Range(4, 20, ErrorMessage = "La cantidad de equipos debe ser 4, 6, 8, 10, 12, 14, 16, 18 o 20")]
        public int CantidadEquipos { get; set; }

        /// <summary>
        /// ID del usuario comisionado (creador)
        /// </summary>
        [Required(ErrorMessage = "El ID del comisionado es obligatorio")]
        public int IdComisionado { get; set; }

        /// <summary>
        /// Nombre del equipo del comisionado
        /// </summary>
        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre del equipo debe tener entre 1 y 100 caracteres")]
        public string NombreEquipoComisionado { get; set; } = string.Empty;

        /// <summary>
        /// Configuración de playoffs: 4 o 6 equipos
        /// </summary>
        [Required(ErrorMessage = "La configuración de playoffs es obligatoria")]
        [Range(4, 6, ErrorMessage = "Los playoffs deben ser de 4 o 6 equipos")]
        public int EquiposEnPlayoffs { get; set; } = 4;
    }

    /// <summary>
    /// DTO para la respuesta de creación de liga
    /// </summary>

}
