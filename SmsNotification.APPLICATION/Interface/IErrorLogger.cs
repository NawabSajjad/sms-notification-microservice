namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for application error logging.
    /// </summary>
    public interface IErrorLogger
    {
        /// <summary>
        /// Logs error details asynchronously.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="errorCode"></param>
        /// <param name="httpStatusCode"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task LogAsync(
            string source,
            string errorCode,
            int httpStatusCode,
            Exception exception);
    }
}