
namespace SmsNotification.DOMAIN.Entities
{
    public class ErrorLog
    {
        /// <summary>
        /// Entity Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Entity source
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Entity errorcode
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Entity Http status code
        /// </summary>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// Entity Message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Entity Stack trace
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Entity CreatedAt
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
