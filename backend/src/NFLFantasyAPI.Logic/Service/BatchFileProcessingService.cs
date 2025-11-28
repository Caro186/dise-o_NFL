using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NFLFantasyAPI.Logic.Interfaces;

namespace NFLFantasyAPI.Logic.Services
{
    /// <summary>
    /// Servicio responsable del manejo y almacenamiento de archivos procesados en batch
    /// </summary>
    public class BatchFileProcessingService : IBatchFileProcessingService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<BatchFileProcessingService> _logger;

        public BatchFileProcessingService(
            IWebHostEnvironment environment,
            ILogger<BatchFileProcessingService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Guarda el archivo procesado en la carpeta correspondiente con el formato requerido
        /// Formato: {timestamp}_{Resultado}_{NombreOriginal}.json
        /// </summary>
        public async Task<string> SaveProcessedFileAsync(
            string originalFileName,
            byte[] fileContent,
            bool success,
            string subfolder)
        {
            try
            {
                // Crear carpeta de archivos procesados si no existe
                var processedFolder = Path.Combine(_environment.WebRootPath, "processed", subfolder);
                if (!Directory.Exists(processedFolder))
                {
                    Directory.CreateDirectory(processedFolder);
                    _logger.LogInformation($"Creada carpeta de archivos procesados: {processedFolder}");
                }

                // Generar nombre del archivo procesado según el formato: <timestamp>_<resultado>_<nombre_original>.json
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
                var extension = Path.GetExtension(originalFileName);

                // Nombre con timestamp y prefijo de resultado para mejor organización
                var resultado = success ? "Exito" : "Fallo";
                var newFileName = $"{timestamp}_{resultado}_{fileNameWithoutExtension}{extension}";
                var fullPath = Path.Combine(processedFolder, newFileName);

                // Guardar el archivo físicamente en disco
                await File.WriteAllBytesAsync(fullPath, fileContent);

                _logger.LogInformation($"Archivo guardado exitosamente en: {fullPath}");

                return newFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo en carpeta de procesados");
                // En caso de error, retornar el nombre original con timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                return $"{timestamp}_Error_{originalFileName}";
            }
        }

        /// <summary>
        /// Lee el contenido de un archivo IFormFile y lo convierte a byte array
        /// </summary>
        public async Task<byte[]> ReadFileContentAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
