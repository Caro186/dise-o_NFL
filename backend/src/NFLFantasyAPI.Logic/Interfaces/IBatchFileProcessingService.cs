using Microsoft.AspNetCore.Http;

namespace NFLFantasyAPI.Logic.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de procesamiento de archivos batch
    /// </summary>
    public interface IBatchFileProcessingService
    {
        /// <summary>
        /// Guarda el archivo procesado en la carpeta correspondiente con timestamp
        /// </summary>
        /// <param name="originalFileName">Nombre original del archivo</param>
        /// <param name="fileContent">Contenido del archivo en bytes</param>
        /// <param name="success">Indica si el procesamiento fue exitoso</param>
        /// <param name="subfolder">Subcarpeta donde guardar (ej: "jugadores")</param>
        /// <returns>Nombre del archivo guardado con timestamp</returns>
        Task<string> SaveProcessedFileAsync(string originalFileName, byte[] fileContent, bool success, string subfolder);

        /// <summary>
        /// Lee el contenido de un archivo y lo convierte a byte array
        /// </summary>
        Task<byte[]> ReadFileContentAsync(IFormFile file);
    }
}
