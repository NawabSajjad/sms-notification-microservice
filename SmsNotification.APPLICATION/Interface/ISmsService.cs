using SmsNotification.APPLICATION.Models;
using SmsNotification.APPLICATION.Request_DTO;

namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for SMS operations.
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Sends an SMS message asynchronously.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        Task<OtpSendResult> SendSmsAsync(
            SmsRequestDto requestDto);
    }
}