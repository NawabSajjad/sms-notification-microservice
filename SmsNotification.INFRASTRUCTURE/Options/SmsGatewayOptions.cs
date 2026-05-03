
namespace SmsNotification.INFRASTRUCTURE.Models
{
    public class SmsGatewayOptions
    {
        /// <summary>
        /// Entity base url
        /// </summary>
        public required string BaseUrl { get; set; }

        /// <summary>
        /// Entity User name
        /// </summary>
        public required string UserName { get; set; }

        /// <summary>
        /// Entity Pin
        /// </summary>
        public required string Pin { get; set; }

        /// <summary>
        /// Entity Sender Id
        /// </summary>
        public required string SenderId { get; set; }

        /// <summary>
        /// Entity Entity Id
        /// </summary>
        public required string EntityId { get; set; }

        /// <summary>
        /// Entity Timeout
        /// </summary>
        public int TimeoutSeconds { get; set; }
    }
}
