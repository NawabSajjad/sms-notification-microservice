namespace SmsNotification.APPLICATION.Models
{
    public class OtpSendResult
    {
        /// <summary>
        /// Entity IsSuccess
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Entity Code
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Entity Message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Method result
        /// </summary>
        /// <param name="success"></param>
        /// <param name="code"></param>
        /// <param name="message"></param>
        private OtpSendResult(bool success, string code, string message)
        {
            IsSuccess = success;
            Code = code;
            Message = message;
        }

        /// <summary>
        /// Method for SMS Send
        /// </summary>
        /// <returns></returns>
        public static OtpSendResult Sent() =>
            new(true, "OTP_SENT", "OTP sent successfully");

        /// <summary>
        /// Method for Already SMS Send
        /// </summary>
        /// <returns></returns>
        public static OtpSendResult AlreadySent() =>
            new(true, "OTP_ALREADY_SENT", "OTP already sent. Please wait before retrying");

        /// <summary>
        /// Method for OTP failed
        /// </summary>
        /// <returns></returns>
        public static OtpSendResult Failed() =>
            new(false, "OTP_FAILED", "Unable to send OTP. Please try again.");

        /// <summary>
        /// Method for otp queue
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static OtpSendResult Queued(string channel) =>
           new(true, "OTP_Queued", $"OTP queued via {channel}");

        /// <summary>
        /// Method for partial send
        /// </summary>
        /// <param name="successChannels"></param>
        /// <param name="failedChannels"></param>
        /// <returns></returns>
        public static OtpSendResult Partial(string successChannels, string failedChannels) =>
          new(true, "OTP_PARTIAL",
        $"Partial success. Success: {successChannels} | Failed: {failedChannels}");
    }
}
