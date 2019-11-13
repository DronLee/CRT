using System.Collections.Generic;
using System.Linq;
using PngProcessorService.Models;
using System.Web.Configuration;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System;
using PngProcessorService.Contracts;

namespace PngProcessorService
{
    /// <summary>
    /// Класс описывает сервис.
    /// </summary>
    [AspNetCompatibilityRequirements (RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service : IService
    {
        private readonly string _workDirectory;
        private readonly List<PngFile> _pngFiles;

        public Service()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~/");
            _workDirectory = configuration.AppSettings.Settings["WorkDirectory"].Value;
            _pngFiles = new List<PngFile>();
        }

        /// <summary>
        /// Отправить на сервер файл.
        /// </summary>
        /// <param name="content">Байты файла.</param>
        /// <returns>Присвоенный файлу идентификатор.</returns>
        public string SendFile(FileRequest file)
        {
            var pngFile = new PngFile(_workDirectory, Convert.FromBase64String(file.ContentBase64));
            _pngFiles.Add(pngFile);
            return pngFile.Id;
        }

        /// <summary>
        /// Начать обработку файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        public void ProcessFile(string fileId)
        {
            GetPngFile(fileId).Process();
        }

        /// <summary>
        /// Получить текущий прогресс обработки файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        /// <returns>Текущее значение прогресса обработки файла.</returns>
        public double GetProgress(string fileId)
        {
            return GetPngFile(fileId).Progress;
        }

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        public void CancelProcess(string fileId)
        {
            GetPngFile(fileId).CancelProcess();
        }

        private PngFile GetPngFile(string fileId)
        {
            var pngFile = _pngFiles.SingleOrDefault(f => f.Id == fileId);
            if (pngFile == null)
                throw new KeyNotFoundException();
            return pngFile;
        }
    }
}