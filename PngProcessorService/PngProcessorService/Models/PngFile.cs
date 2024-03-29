﻿using ImageProcessor;
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
    internal class PngFile : IFile
    {
        private readonly string _filePath;

        private Thread _processThread;
        private object processLocker = new object();

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="workDirectory">Рабочая директория, в которой будет сохранён поступивший файл.</param>
        /// <param name="content">Байты поступившего файла.</param>
        public PngFile(string workDirectory, byte[] content)
        {
            Id = Guid.NewGuid().ToString();
            _filePath = Path.Combine(workDirectory, Id);
            File.WriteAllBytes(_filePath, content);
            Progress = 0;
        }

        /// <summary>
        /// Идентификатор, присвоенный файлу. 
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Текущее значение прогресса обработки файла.
        /// </summary>
        public double Progress { get; private set; }

        /// <summary>
        /// Событие завершения обработки файла. Передаёт файл, обработка которого завершена.
        /// </summary>
        public event Action<IFile> ProcessedEvent;

        /// <summary>
        /// Начать обработку файла.
        /// </summary>
        public void Process()
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
                    try
                    {
                        using (var pngProcessor = new PngProcessor())
                        {
                            pngProcessor.ProgressChanged += (double progress) => { Progress = progress; };
                            pngProcessor.Process(_filePath);
                        }
                    }
                    finally
                    {
                        ProcessedEvent?.Invoke(this);
                    }
                });
                _processThread.Start();
            }
        }

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        public void CancelProcess()
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