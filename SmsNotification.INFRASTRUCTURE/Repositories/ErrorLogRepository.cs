using SmsNotification.APPLICATION.Interface;
using SmsNotification.DOMAIN.Entities;
using SmsNotification.INFRASTRUCTURE.Data;
using Dapper;

namespace SmsNotification.INFRASTRUCTURE.Repositories
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        /// <summary>
        /// DI for Connection
        /// </summary>
        /// <param name="connectionFactory"></param>
        public ErrorLogRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Method for Log in db
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task LogAsync(ErrorLog log)
        {
            try
            {
                using var conn = _connectionFactory.CreateConnection();

                var parameters = new
                {
                    p_source = log.Source,
                    p_error_code = log.ErrorCode,
                    p_http_status_code = log.HttpStatusCode,
                    p_message = log.Message,
                    p_stack_trace = log.StackTrace
                };

                await conn.ExecuteAsync(
                    "CALL proc_insert_error_log(@p_source, @p_error_code, @p_http_status_code, @p_message, @p_stack_trace)",
                    parameters);
            }
            catch
            {
                // NEVER THROW FROM ERROR LOGGING
            }
        }
    }
}
