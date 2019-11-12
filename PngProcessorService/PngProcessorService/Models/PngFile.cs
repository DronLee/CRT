﻿using ImageProcessor;
using System;
using System.IO;
using System.Threading;

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
        /// Начать обработку файла.
        /// </summary>
        internal void Process()
        {
            lock (processLocker)
            {
                if (_processThread != null)
                    throw new ProcessIsAlreadyRunningException();

                _processThread = new Thread(() =>
                {
                    using (var pngProcessor = new PngProcessor())
                    {
                        pngProcessor.ProgressChanged += (double progress) => { Progress = progress; };
                        pngProcessor.Process(_filePath);
                    }
                });
                _processThread.Start();
            }
        }

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        internal void CancelProcess()
        {
            lock(processLocker)
            {
                if (_processThread == null)
                    throw new ProcessIsNotRunningException();

                _processThread.Abort();
                _processThread = null;
            }
        }
    }
}