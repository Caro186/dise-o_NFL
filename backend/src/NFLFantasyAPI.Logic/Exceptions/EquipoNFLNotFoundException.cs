namespace NFLFantasyAPI.Logic.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando no se encuentra un equipo NFL
    /// </summary>
    public class EquipoNFLNotFoundException : Exception
    {
        public int EquipoNFLId { get; }

        public EquipoNFLNotFoundException(int equipoNFLId)
            : base($"Equipo NFL con ID {equipoNFLId} no fue encontrado")
        {
            EquipoNFLId = equipoNFLId;
        }

        public EquipoNFLNotFoundException(int equipoNFLId, string message)
            : base(message)
        {
            EquipoNFLId = equipoNFLId;
        }
    }
}
