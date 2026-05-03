namespace SmsNotification.APPLICATION.Request_DTO
{
    /// <summary>
    /// Represents a request to send an SMS, including OTP, recipient details, client information, template, status, channels, and cancellation support.
    /// </summary>
    public record SmsRequestDto(
        string otp,
        string mobile,
        string clientIP,
        string flag,
        string pageName,
        string countryMobileCode,
        string userName,
        string templateName,
        int NotificationStatus,
        int smsStatus,
        List<string> channels,
        CancellationToken cancellationToken
    );
}