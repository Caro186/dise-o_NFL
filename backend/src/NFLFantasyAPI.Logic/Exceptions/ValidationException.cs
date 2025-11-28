namespace NFLFantasyAPI.Logic.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando falla la validación de datos
    /// </summary>
    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errores { get; }

        public ValidationException(string message) : base(message)
        {
            Errores = new Dictionary<string, string>();
        }

        public ValidationException(string campo, string mensaje)
            : base($"Error de validación en {campo}: {mensaje}")
        {
            Errores = new Dictionary<string, string> { { campo, mensaje } };
        }

        public ValidationException(Dictionary<string, string> errores)
            : base("Se encontraron errores de validación")
        {
            Errores = errores;
        }
    }
}
