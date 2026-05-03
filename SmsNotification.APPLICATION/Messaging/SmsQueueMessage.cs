namespace SmsNotification.APPLICATION.Messaging
{
    /// <summary>
    /// Represents a message to be enqueued and processed for SMS delivery,
    /// containing all necessary information such as recipient, content, template, status, and channel details.
    /// </summary>
    public record SmsQueueMessage(
       long smsId,
       string mobile,
       string message,
       string templateId,
       string correlationId,
       int NotificationStatus,
       string channel
   );
}