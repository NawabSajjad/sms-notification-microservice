using SmsNotification.DOMAIN.Entities;

namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for storing error logs.
    /// </summary>
    public interface IErrorLogRepository
    {
        /// <summary>
        /// Logs error details asynchronously.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        Task LogAsync(ErrorLog log);
    }
}