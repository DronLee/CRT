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
        private readonly PngFileProcessor _fileProcessor;

        public Service()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~/");
            _fileProcessor = new PngFileProcessor(configuration.AppSettings.Settings["WorkDirectory"].Value,
                short.Parse(configuration.AppSettings.Settings["ProcessPoolSize"].Value));
        }

        /// <summary>
        /// Отправить на сервер файл.
        /// </summary>
        /// <param name="content">Байты файла.</param>
        /// <returns>Присвоенный файлу идентификатор.</returns>
        public string SendFile(FileRequest file)
        {
            return _fileProcessor.AddFile(Convert.FromBase64String(file.ContentBase64));
        }

        /// <summary>
        /// Начать обработку файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        public void ProcessFile(string fileId)
        {
            _fileProcessor.ProcessFile(fileId);
        }

        /// <summary>
        /// Получить текущий прогресс обработки файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        /// <returns>Текущее значение прогресса обработки файла.</returns>
        public double GetProgress(string fileId)
        {
            return _fileProcessor.GetPngFile(fileId).Progress;
        }

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        public void CancelProcess(string fileId)
        {
            _fileProcessor.CancelProcess(fileId);
        }
    }
}