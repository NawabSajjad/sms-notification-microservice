
using System.Text.RegularExpressions;

namespace SmsNotification.APPLICATION.Utilities
{
    public static class MaskingHelper
    {
        /// <summary>
        /// Method for Mobile no. masking
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static string MaskMobile(string? mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return "EMPTY";

            if (mobile.Length <= 4)
                return "****";

            return mobile[..Math.Min(6, mobile.Length)] + "****";
        }

        /// <summary>
        /// OTP Mask
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string MaskOtp(string message)
        {
            return Regex.Replace(message, @"\d{4,8}", "****");
        }
    }
}
