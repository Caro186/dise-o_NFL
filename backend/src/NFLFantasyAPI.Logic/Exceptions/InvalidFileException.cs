namespace NFLFantasyAPI.Logic.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando un archivo no es válido
    /// </summary>
    public class InvalidFileException : Exception
    {
        public string? FileName { get; }

        public InvalidFileException(string message) : base(message)
        {
        }

        public InvalidFileException(string fileName, string message)
            : base(message)
        {
            FileName = fileName;
        }

        public InvalidFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
