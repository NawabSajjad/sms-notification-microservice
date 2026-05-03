namespace SmsNotification.INFRASTRUCTURE.Options
{
    public class NotificationGatewayOptions
    {
        /// <summary>
        /// entity Baseurl
        /// </summary>
        public string BaseUrl { get; set; } = default!;

        /// <summary>
        /// entity Priority
        /// </summary>
        public string Priority { get; set; } = "high-volatile";

        /// <summary>
        /// entity Timeout
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }
}
