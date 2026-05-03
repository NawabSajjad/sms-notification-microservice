namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for sending SMS messages through gateway services.
    /// </summary>
    public interface ISmsGateway
    {
        /// <summary>
        /// Sends an SMS message asynchronously.
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <param name="message"></param>
        /// <param name="templateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(bool isSuccess, string response)> SendAsync(
           string mobileNo,
           string message,
           string templateId,
           CancellationToken cancellationToken);
    }
}