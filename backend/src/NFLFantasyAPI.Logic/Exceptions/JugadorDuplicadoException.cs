namespace NFLFantasyAPI.Logic.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se intenta crear un jugador duplicado
    /// </summary>
    public class JugadorDuplicadoException : Exception
    {
        public string NombreJugador { get; }
        public int EquipoNFLId { get; }

        public JugadorDuplicadoException(string nombreJugador, int equipoNFLId)
            : base($"Ya existe un jugador con el nombre '{nombreJugador}' en el equipo NFL con ID {equipoNFLId}")
        {
            NombreJugador = nombreJugador;
            EquipoNFLId = equipoNFLId;
        }

        public JugadorDuplicadoException(string message) : base(message)
        {
            NombreJugador = string.Empty;
        }
    }
}
