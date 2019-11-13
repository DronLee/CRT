using ImageProcessor;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("UnitTestProject")]
namespace PngProcessorService.Models
{
    /// <summary>
    /// Класс описывает поступающий в обработку файл.
    /// </summary>
    internal class PngFile
    {
        private readonly string _filePath;

        private Thread _processThread;
        private object processLocker = new object();

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="workDirectory">Рабочая директория, в которой будет сохранён поступивший файл.</param>
        /// <param name="content">Байты поступившего файла.</param>
        internal PngFile(string workDirectory, byte[] content)
        {
            Id = Guid.NewGuid().ToString();
            _filePath = Path.Combine(workDirectory, Id);
            File.WriteAllBytes(_filePath, content);
            Progress = 0;
        }

        /// <summary>
        /// Идентификатор, присвоенный файлу. 
        /// </summary>
        internal string Id { get; }

        /// <summary>
        /// Текущее значение прогресса обработки файла.
        /// </summary>
        internal double Progress { get; private set; }

        /// <summary>
        /// Событие завершения обработки файла.
        /// </summary>
        internal event Action ProcessedEvent;

        /// <summary>
        /// Начать обработку файла.
        /// </summary>
        internal void Process()
        {
            lock (processLocker)
            {
                if (_processThread != null)
                    if (_processThread.ThreadState == ThreadState.Stopped)
                        throw new FileIsAlreadyProcessedException();
                    else
                        throw new ProcessIsAlreadyRunningException();

                _processThread = new Thread(() =>
                {
                    using (var pngProcessor = new PngProcessor())
                    {
                        pngProcessor.ProgressChanged += (double progress) => { Progress = progress; };
                        pngProcessor.Process(_filePath);
                    }
                    ProcessedEvent?.Invoke();
                });
                _processThread.Start();
            }
        }

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        internal void CancelProcess()
        {
            lock (processLocker)
            {
                if (_processThread == null)
                    throw new ProcessIsNotRunningException();
                else if (_processThread.ThreadState != ThreadState.Stopped)
                {
                    _processThread.Abort();
                    _processThread = null;
                    Progress = 0;
                }
            }
        }
    }
}