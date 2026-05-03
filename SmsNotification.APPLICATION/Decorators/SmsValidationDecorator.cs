using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Models;
using SmsNotification.APPLICATION.Request_DTO;
using SmsNotification.DOMAIN;
using System.Text.RegularExpressions;

namespace SmsNotification.APPLICATION.Decorators
{
    public class SmsValidationDecorator : ISmsService
    {
        private readonly ISmsService _inner;

        private static readonly Regex mobileRegex =
            new(@"^[6-9]\d{9}$", RegexOptions.Compiled);


        /// <summary>
        /// Initializes SmsValidationDecorator dependencies.
        /// </summary>
        /// <param name="inner"></param>
        public SmsValidationDecorator(ISmsService inner)
        {
            _inner = inner;
        }

        /// <summary>
        /// Sends SMS after request validation.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        public Task<OtpSendResult> SendSmsAsync(SmsRequestDto requestDto)
        {
            Validate(requestDto);
            return _inner.SendSmsAsync(requestDto);
        }

        /// <summary>
        /// Validates SMS request details.
        /// </summary>
        /// <param name="dto"></param>
        /// <exception cref="BusinessException"></exception>
        private static void Validate(SmsRequestDto dto)
        {
            if (dto is null)
                throw new BusinessException("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.mobile))
                throw new BusinessException("Mobile number is required");

            if (!mobileRegex.IsMatch(dto.mobile))
                throw new BusinessException("Invalid mobile number");

            if (string.IsNullOrWhiteSpace(dto.otp))
                throw new BusinessException("OTP is required");

            if (dto.otp.Length < 4 || dto.otp.Length > 8)
                throw new BusinessException("OTP length must be between 4 and 8");

            if (string.IsNullOrWhiteSpace(dto.pageName))
                throw new BusinessException("Page name is required");

            if (dto.channels == null || !dto.channels.Any())
                throw new BusinessException("At least one channel is required");

            var validChannels = new[] { "SMS", "Notification" };

            if (dto.channels.Any(c => !validChannels.Contains(c.ToUpper())))
                throw new BusinessException("Invalid channel provided");
        }
    }
}
