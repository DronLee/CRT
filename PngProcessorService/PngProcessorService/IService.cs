using System.ServiceModel;
using System.ServiceModel.Web;

namespace PngProcessorService
{
    /// <summary>
    /// Интерфейс сервиса.
    /// </summary>
    [ServiceContract]
    public interface IService
    {
        /// <summary>
        /// Отправить на сервер файл.
        /// </summary>
        /// <param name="content">Байты файла.</param>
        /// <returns>Присвоенный файлу идентификатор.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/send", Method = "POST")]
        string SendFile(byte[] content);

        /// <summary>
        /// Начать обработку файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "/process/{fileId}", Method = "PUT")]
        void ProcessFile(string fileId);

        /// <summary>
        /// Получить текущий прогресс обработки файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        /// <returns>Текущее значение прогресса обработки файла.</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/process/{fileId}")]
        double GetProgress(string fileId);

        /// <summary>
        /// Отменить обработку файла.
        /// </summary>
        /// <param name="fileId">Идентификатор файла.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "/process/{fileId}", Method = "DELETE")]
        void CancelProcess(string fileId);
    }
}