
namespace SmsNotification.DOMAIN.Entities
{
    public class Sms
    {
        /// <summary>
        /// Entity Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Entity Otp
        /// </summary>
        public string Otp { get; set; } = string.Empty;

        /// <summary>
        /// Entity Mobile
        /// </summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// Entity UserName
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Entity Result
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// Entity Message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Entity NotificationStatus
        /// </summary>
        public int NotificationStatus { get; set; }

        /// <summary>
        /// Entity NotificationStatus
        /// </summary>
        public int SmsStatus { get; set; }

        /// <summary>
        /// Entity NotificationResponse
        /// </summary>
        public string NotificationResponse { get; set; } = string.Empty;

        /// <summary>
        /// Method for pass value to main entity
        /// </summary>
        /// <param name="otp"></param>
        /// <param name="mobile"></param>
        /// <exception cref="BusinessException"></exception>
        public Sms(string otp, string mobile, int NotificationStatus, int smsStatus)
        {
            if (string.IsNullOrWhiteSpace(otp))
                throw new BusinessException("Otp number required");

            if (string.IsNullOrWhiteSpace(mobile))
                throw new BusinessException("Mobile number required");

            Mobile = mobile.Trim();
            Otp = otp.Trim();
            NotificationStatus = 0; // Pending
            smsStatus = 0;// Pending
        }

    }
}
