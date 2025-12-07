using NFLFantasyAPI.Logic.Exceptions;

namespace NFLFantasyAPI.Logic.Validators
{
    /// <summary>
    /// Validador compartido para validaciones de archivos e imágenes
    /// REFACTORIZADO: Extraído de validaciones duplicadas en múltiples servicios
    /// </summary>
    public static class FileValidator
    {
        #region Validaciones de Archivos de Imagen

        /// <summary>
        /// Valida un archivo de imagen
        /// </summary>
        public static void ValidarArchivoImagen(string? contentType, long fileLength, long maxSizeMB = 5)
        {
            ValidarArchivoNoVacio(fileLength);
            ValidarTipoImagen(contentType);
            ValidarTamañoArchivo(fileLength, maxSizeMB);
        }

        /// <summary>
        /// Valida que el archivo no esté vacío
        /// </summary>
        private static void ValidarArchivoNoVacio(long fileLength)
        {
            if (fileLength == 0)
                throw new InvalidFileException("El archivo está vacío");
        }

        /// <summary>
        /// Valida que el archivo sea de un tipo de imagen permitido
        /// </summary>
        private static void ValidarTipoImagen(string? contentType)
        {
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            if (string.IsNullOrWhiteSpace(contentType) || !allowedTypes.Contains(contentType.ToLower()))
                throw new InvalidFileException("Solo se permiten imágenes JPEG o PNG");
        }

        /// <summary>
        /// Valida que el tamaño del archivo no exceda el límite
        /// </summary>
        private static void ValidarTamañoArchivo(long fileLength, long maxSizeMB)
        {
            var maxSizeBytes = maxSizeMB * 1024 * 1024;
            if (fileLength > maxSizeBytes)
                throw new InvalidFileException($"El tamaño máximo permitido es {maxSizeMB} MB");
        }

        #endregion
    }
}
