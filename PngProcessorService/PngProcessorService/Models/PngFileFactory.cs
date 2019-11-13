namespace PngProcessorService.Models
{
    /// <summary>
    /// Фабрика создания моделей для png-файлов.
    /// </summary>
    internal class PngFileFactory : IFileFactory
    {
        private readonly string _workDirectory;

        internal PngFileFactory(string workDirectory)
        {
            _workDirectory = workDirectory;
        }

        /// <summary>
        /// Создание модели файла.
        /// </summary>
        /// <param name="content">Байты файла.</param>
        /// <returns>Созданная модель файла.</returns>
        public IFile CreateFile(byte[] content)
        {
            return new PngFile(_workDirectory, content);
        }
    }
}