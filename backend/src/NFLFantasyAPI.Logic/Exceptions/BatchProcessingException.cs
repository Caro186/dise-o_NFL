namespace NFLFantasyAPI.Logic.Exceptions
{
    /// <summary>
    /// Excepción lanzada durante el procesamiento batch
    /// </summary>
    public class BatchProcessingException : Exception
    {
        public int TotalErrores { get; }
        public List<string> Errores { get; }

        public BatchProcessingException(string message, int totalErrores = 0)
            : base(message)
        {
            TotalErrores = totalErrores;
            Errores = new List<string>();
        }

        public BatchProcessingException(string message, List<string> errores)
            : base(message)
        {
            TotalErrores = errores.Count;
            Errores = errores;
        }

        public BatchProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
            TotalErrores = 0;
            Errores = new List<string>();
        }
    }
}
