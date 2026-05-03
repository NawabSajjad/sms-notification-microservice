using SmsNotification.DOMAIN.Entities;

namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for SMS template operations.
    /// </summary>
    public interface ISmsTemplateRepository
    {
        /// <summary>
        /// Retrieves an SMS template asynchronously.
        /// </summary>
        /// <param name="templateCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<SmsTemplate?> GetByTemplateNameAsync(
            string templateCode,
            CancellationToken cancellationToken = default);
    }
}