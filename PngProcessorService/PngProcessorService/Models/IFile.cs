﻿using System;

namespace PngProcessorService.Models
{
    /// <summary>
    /// Интерфейс описывает обрабатываемый файл.
    /// </summary>
    internal interface IFile
    {
        /// <summary>
        /// Идентификатор, присвоенный файлу. 
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Текущее значение прогресса обработки файла.
        /// </summary>
        double Progress { get; }

        /// <summary>
        /// Событие завершения обработки файла.
        /// </summary>
        event Action ProcessedEvent;

        /// <summary>
        /// Начать обработку файла.
        /// </summary>
        void Process();

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        void CancelProcess();
    }
}