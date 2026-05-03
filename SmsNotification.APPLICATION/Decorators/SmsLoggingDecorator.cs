using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Models;
using SmsNotification.APPLICATION.Request_DTO;
using SmsNotification.APPLICATION.Utilities;
using Microsoft.Extensions.Logging;

namespace SmsNotification.APPLICATION.Decorators
{
    public class SmsLoggingDecorator : ISmsService
    {
        private readonly ISmsService _inner;
        private readonly ILogger<SmsLoggingDecorator> _logger;

        /// <summary>
        /// Initializes SmsLoggingDecorator dependencies.
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="logger"></param>
        public SmsLoggingDecorator(ISmsService inner, ILogger<SmsLoggingDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>
        /// Sends SMS with logging asynchronously.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        public async Task<OtpSendResult> SendSmsAsync(SmsRequestDto requestDto)
        {
            _logger.LogInformation(
                 "OTP request for {Mobile} Channels:{Channels} Page:{Page}",
                 MaskingHelper.MaskMobile(requestDto.mobile),
                 string.Join(",", requestDto.channels),
                 requestDto.pageName);

            var result = await _inner.SendSmsAsync(requestDto);
            _logger.LogInformation("OTP processing completed with status: {result}",result.Message);
            return result;
        }
    }
}
