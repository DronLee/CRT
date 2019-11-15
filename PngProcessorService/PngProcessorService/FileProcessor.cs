using PngProcessorService.Models;
using System.Collections.Generic;
using System.Linq;

namespace PngProcessorService
{
    /// <summary>
    /// Модель обработчика файлов.
    /// </summary>
    internal class FileProcessor
    {
        /// <summary>
        /// Фабрика создания моделей файлов.
        /// </summary>
        private readonly IFileFactory _fileFactory;
        /// <summary>
        /// Разрешённый размер пула обрабатываемых файлов.
        /// </summary>
        private readonly short _processPoolSize;
        /// <summary>
        /// Словарь поступивших файлов, по их идентификаторам.
        /// </summary>
        private readonly Dictionary<string, IFile> _files;

        /// <summary>
        /// Очередь ожидающих обработки файлов. 
        /// </summary>
        private readonly List<IFile> processFilesMQ;
        /// <summary>
        /// Объект для блокировки обращений к очереди.
        /// </summary>
        private object processMQLocker = new object();
        /// <summary>
        /// Количество обрабатываемых в данный момент файлов.
        /// </summary>
        private short _processingFilesCount;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="fileFactory">Фабрика создания моделей файлов.</param>
        /// <param name="processPoolSize">Разрешённый размер пула обрабатываемых файлов. Если в обработку поступает больше файлов, они становятся в очередь.</param>
        internal FileProcessor(IFileFactory fileFactory, short processPoolSize)
        {
            _fileFactory = fileFactory;
            _processPoolSize = processPoolSize;
            _files = new Dictionary<string, IFile>();
            processFilesMQ = new List<IFile>();
        }

        /// <summary>
        /// Добавление файла.
        /// </summary>
        /// <param name="contentFile">Байты добавляемого файла.</param>
        /// <returns>Идентификатор, присвоенный файлу.</returns>
        internal string AddFile(byte[] contentFile)
        {
            var file = _fileFactory.CreateFile(contentFile);
            _files.Add(file.Id, file);
            return file.Id;
        }

        /// <summary>
        /// Начать обработку файла. Если уже обрабатывается файлов больше, чем позволено, файл будет поставлен в очередь на обработку. 
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        internal void ProcessFile(string fileId)
        {
            var file = GetFile(fileId);
            
            lock (processMQLocker)
            {
                if (_processingFilesCount < _processPoolSize)
                {
                    _processingFilesCount++;
                    ProcessFile(file);
                }
                else
                    processFilesMQ.Add(file);
            }
        }

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        internal void CancelProcess(string fileId)
        {
            var file = GetFile(fileId);

            // Сначала проверим, может файл в очереди на обработку стоит, тогда достаточно его просто из очереди убрать.
            bool removedFromMQ = false;
            lock (processMQLocker)
                removedFromMQ = processFilesMQ.Remove(file);

            if (!removedFromMQ)
            {
                file.CancelProcess();
                ProcessingStopped(file);
            }
        }

        /// <summary>
        /// Получение файла по его идентификатору.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        /// <returns>Модель файла.</returns>
        internal IFile GetFile(string fileId)
        {
            if (!_files.ContainsKey(fileId))
                throw new KeyNotFoundException();
            return _files[fileId];
        }

        private void ProcessingStopped(IFile file)
        {
            file.ProcessedEvent -= ProcessingStopped; // Отписываемся, так как если выполняется этот метот, то обработки файла ждать не стоит.

            lock (processMQLocker)
                if (processFilesMQ.Count > 0)
                {
                    var processFile = processFilesMQ.First();
                    ProcessFile(processFile);
                    processFilesMQ.Remove(processFile);
                }
                else
                    _processingFilesCount--;

        }

        private void ProcessFile(IFile file)
        {
            file.ProcessedEvent += ProcessingStopped; // Перед обработкой подписываемся на событие в ожидании завершения.

            file.Process();
        }
    }
}