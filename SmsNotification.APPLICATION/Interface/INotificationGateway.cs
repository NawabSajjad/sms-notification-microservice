namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for sending messages through Notification gateway.
    /// </summary>
    public interface INotificationGateway
    {
        /// <summary>
        /// Sends a message through the Notification gateway asynchronously.
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(bool isSuccess, string response)> SendAsync(
            string mobile,
            string message,
            CancellationToken cancellationToken);
    }
}