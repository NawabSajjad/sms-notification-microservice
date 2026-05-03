namespace SmsNotification.INFRASTRUCTURE.Options
{
    public class RabbitMqOptions
    {
        /// <summary>
        /// entity Section 
        /// </summary>
        public const string SectionName = "RabbitMq";

        /// <summary>
        /// entity host
        /// </summary>
        public string Host { get; set; } = default!;

        /// <summary>
        /// entity port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// entity username
        /// </summary>
        public string Username { get; set; } = default!;

        /// <summary>
        /// entity password
        /// </summary>
        public string Password { get; set; } = default!;

        /// <summary>
        /// entity virtual host
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// entity Queue Name
        /// </summary>
        public string QueueName { get; set; } = default!;

        /// <summary>
        /// entity exchange name
        /// </summary>
        public string ExchangeName { get; set; } = "otp_exchange";

        /// <summary>
        /// entity sms queue
        /// </summary>
        public string SmsQueue { get; set; } = "sms_queue";

        /// <summary>
        /// entity sms routing
        /// </summary>
        public string SmsRoutingKey { get; set; } = "sms";

        /// <summary>
        /// entity smsDlq
        /// </summary>
        public string SmsDlq { get; set; } = "sms_queue.dlq";

        /// <summary>
        /// entity Notification queue
        /// </summary>
        public string NotificationQueue { get; set; } = "Notification_queue";

        /// <summary>
        /// entity Notification routing key
        /// </summary>
        public string NotificationRoutingKey { get; set; } = "Notification";

        /// <summary>
        /// entity Notification dlq
        /// </summary>
        public string NotificationDlq { get; set; } = "Notification_queue.dlq";
    }
}
