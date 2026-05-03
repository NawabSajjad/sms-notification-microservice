using SmsNotification.APPLICATION.Interface;
using SmsNotification.DOMAIN.Entities;

namespace SmsNotification.APPLICATION.Service
{
    /// <summary>
    /// Provides functionality to log application errors by implementing the IErrorLogger interface.
    /// This service creates error log entries and delegates their persistence to the configured error log repository.
    /// </summary>
    public class ErrorLoggerService : IErrorLogger
    {
        private readonly IErrorLogRepository _repository;

        /// <summary>
        /// Method for logger
        /// </summary>
        /// <param name="repository"></param>
        public ErrorLoggerService(IErrorLogRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Method for error LogAsync
        /// </summary>
        /// <param name="source"></param>
        /// <param name="errorCode"></param>
        /// <param name="httpStatusCode"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public async Task LogAsync(
            string source,
            string errorCode,
             int httpStatusCode,
            Exception exception)
        {
            var log = new ErrorLog
            {
                Source = source,
                ErrorCode = errorCode,
                HttpStatusCode = httpStatusCode,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.LogAsync(log);
        }
    }
}
