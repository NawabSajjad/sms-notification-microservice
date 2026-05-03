using SmsNotification.DOMAIN.Entities;

namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for managing SMS records and statuses.
    /// </summary>
    public interface ISmsRepository
    {
        /// <summary>
        /// Inserts a new SMS record asynchronously.
        /// </summary>
        /// <param name="sms"></param>
        /// <param name="smsAuditMeta"></param>
        /// <returns></returns>
        Task<(long smsId, string result)> InsertSmsAsync(
            Sms sms,
            SmsAuditInfo smsAuditMeta);

        /// <summary>
        /// Updates SMS status asynchronously.
        /// </summary>
        /// <param name="smsId"></param>
        /// <param name="status"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<string> UpdateSmsStatusAsync(
            long smsId,
            int status,
            string response);

        /// <summary>
        /// Updates Notification status asynchronously.
        /// </summary>
        /// <param name="smsId"></param>
        /// <param name="status"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<string> UpdateNotificationStatusAsync(
            long smsId,
            int status,
            string response);
    }
}