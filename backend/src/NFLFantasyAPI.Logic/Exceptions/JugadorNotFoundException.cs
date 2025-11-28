namespace NFLFantasyAPI.Logic.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando no se encuentra un jugador
    /// </summary>
    public class JugadorNotFoundException : Exception
    {
        public int JugadorId { get; }

        public JugadorNotFoundException(int jugadorId)
            : base($"Jugador con ID {jugadorId} no fue encontrado")
        {
            JugadorId = jugadorId;
        }

        public JugadorNotFoundException(int jugadorId, string message)
            : base(message)
        {
            JugadorId = jugadorId;
        }
    }
}
