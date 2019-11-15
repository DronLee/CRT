using PngProcessorService.Models;
using System.Collections.Generic;
using System.Linq;

namespace PngProcessorService
{
    internal class PngFileProcessor
    {
        private readonly IFileFactory _fileFactory;
        private readonly short _processPoolSize;
        private readonly Dictionary<string, IFile> _pngFiles;

        private readonly List<IFile> processFilesMQ;
        private object processMQLocker = new object();
        private short _processingFilesCount;

        /// <summary>
        /// Консруктор.
        /// </summary>
        /// <param name="fileFactory">Фабрика создания моделей файлов.</param>
        /// <param name="processPoolSize">Разрешённый размер пула обрабатываемых файлов. Если в обработку поступает больше файлов, они становятся в очередь.</param>
        internal PngFileProcessor(IFileFactory fileFactory, short processPoolSize)
        {
            _fileFactory = fileFactory;
            _processPoolSize = processPoolSize;
            _pngFiles = new Dictionary<string, IFile>();
            processFilesMQ = new List<IFile>();
        }

        /// <summary>
        /// Добавление файла.
        /// </summary>
        /// <param name="contentFile">Байты добавляемого файла.</param>
        /// <returns>Идентификатор, присвоенный файлу.</returns>
        internal string AddFile(byte[] contentFile)
        {
            var pngFile = _fileFactory.CreateFile(contentFile);
            _pngFiles.Add(pngFile.Id, pngFile);
            return pngFile.Id;
        }

        /// <summary>
        /// Начать обработку файла. Если уже обрабатывается файлов больше, чем позволено, файл будет поставлен в очередь на обработку. 
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        internal void ProcessFile(string fileId)
        {
            var pngFile = GetPngFile(fileId);
            
            lock (processMQLocker)
            {
                if (_processingFilesCount < _processPoolSize)
                {
                    _processingFilesCount++;
                    ProcessFile(pngFile);
                }
                else
                    processFilesMQ.Add(pngFile);
            }
        }

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        internal void CancelProcess(string fileId)
        {
            var pngFile = GetPngFile(fileId);

            // Сначала проверим, может файл в очереди на обработку стоит, тогда достаточно его просто из очереди убрать.
            bool removedFromMQ = false;
            lock (processMQLocker)
                removedFromMQ = processFilesMQ.Remove(pngFile);

            if (!removedFromMQ)
            {
                pngFile.CancelProcess();
                ProcessingStopped(pngFile);
            }
        }

        /// <summary>
        /// Получение файла по его идентификатору.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        /// <returns>Модель файла.</returns>
        internal IFile GetPngFile(string fileId)
        {
            if (!_pngFiles.ContainsKey(fileId))
                throw new KeyNotFoundException();
            return _pngFiles[fileId];
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
            file.ProcessedEvent += ProcessingStopped; // Перед обработкой подписываемя на событие в ожидании завершения.

            file.Process();
        }
    }
}