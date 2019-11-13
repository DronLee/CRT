namespace PngProcessorService.Models
{
    /// <summary>
    /// Интерфейс фабрики создания моделей обрабатываемых файлов.
    /// </summary>
    internal interface IFileFactory
    {
        /// <summary>
        /// Создание модели обрабатываемого файла.
        /// </summary>
        /// <param name="content">Байты файла.</param>
        /// <returns>Созданная модель файла.</returns>
        IFile CreateFile(byte[] content);
    }
}