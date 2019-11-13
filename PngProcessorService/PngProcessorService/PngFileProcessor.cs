using PngProcessorService.Models;
using System.Collections.Generic;
using System.Linq;

namespace PngProcessorService
{
    internal class PngFileProcessor
    {
        private readonly string _workDirectory;
        private readonly short _processPoolSize;
        private readonly Dictionary<string, PngFile> _pngFiles;

        private readonly List<PngFile> processFilesMQ;
        private object processMQLocker = new object();
        private short _processingFilesCount;

        /// <summary>
        /// Консруктор.
        /// </summary>
        /// <param name="workDirectory">Рабочая директория, в которую будут сохраняться файлы.</param>
        /// <param name="processPoolSize">Разрешённый размер пула обрабатываемых файлов. Если в обработку поступает больше файлов, они становятся в очередь.</param>
        internal PngFileProcessor(string workDirectory, short processPoolSize)
        {
            _workDirectory = workDirectory;
            _processPoolSize = processPoolSize;
            _pngFiles = new Dictionary<string, PngFile>();
            processFilesMQ = new List<PngFile>();
        }

        /// <summary>
        /// Добавление файла.
        /// </summary>
        /// <param name="contentFile">Байты добавляемого файла.</param>
        /// <returns>Идентификатор, присвоенный файлу.</returns>
        internal string AddFile(byte[] contentFile)
        {
            var pngFile = new PngFile(_workDirectory, contentFile);
            pngFile.ProcessedEvent += ProcessingStopped;
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
                    pngFile.Process();
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
                ProcessingStopped();
            }
        }

        /// <summary>
        /// Получение файла по его идентификатору.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        /// <returns>Модель файла.</returns>
        internal PngFile GetPngFile(string fileId)
        {
            if (!_pngFiles.ContainsKey(fileId))
                throw new KeyNotFoundException();
            return _pngFiles[fileId];
        }

        private void ProcessingStopped()
        {
            lock (processMQLocker)
                if (processFilesMQ.Count > 0)
                {
                    var processFile = processFilesMQ.First();
                    processFile.Process();
                    processFilesMQ.Remove(processFile);
                }
                else
                    _processingFilesCount--;
        }
    }
}