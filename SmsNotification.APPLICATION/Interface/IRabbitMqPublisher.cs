using SmsNotification.APPLICATION.Messaging;

namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for publishing messages to RabbitMQ.
    /// </summary>
    public interface IRabbitMqPublisher
    {
        /// <summary>
        /// Publishes a message to RabbitMQ asynchronously.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<bool> PublishAsync(SmsQueueMessage message);
    }
}