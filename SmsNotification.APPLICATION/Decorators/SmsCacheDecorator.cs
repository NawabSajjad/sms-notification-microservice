using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Models;
using SmsNotification.APPLICATION.Request_DTO;
using SmsNotification.APPLICATION.Utilities;
using Microsoft.Extensions.Logging;

namespace SmsNotification.APPLICATION.Decorators
{
    public class SmsCacheDecorator : ISmsService
    {
        private readonly ISmsService _inner;
        private readonly ICacheService _cache;
        private readonly ILogger<SmsCacheDecorator> _logger;

        /// <summary>
        /// Initializes SmsCacheDecorator dependencies.
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="cache"></param>
        /// <param name="logger"></param>
        public SmsCacheDecorator(
            ISmsService inner,
            ICacheService cache,
            ILogger<SmsCacheDecorator> logger)
        {
            _inner = inner;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Sends SMS with cache validation asynchronously.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        public async Task<OtpSendResult> SendSmsAsync(SmsRequestDto requestDto)
        {
            string key = $"otp:{requestDto.mobile}";
            var maskedMobile = MaskingHelper.MaskMobile(requestDto.mobile);

            var isLocked = await _cache.SetIfNotExistsAsync(
                key,
                requestDto.otp,
                TimeSpan.FromMinutes(5)
            );

            if (!isLocked)
            {
                _logger.LogInformation(
                    "Cache HIT (LOCKED) - OTP already exists for Mobile: {Mobile}",
                    maskedMobile);

                return OtpSendResult.AlreadySent();
            }

            _logger.LogInformation(
                "Cache LOCK ACQUIRED - Sending OTP for Mobile: {Mobile}",
                maskedMobile);

            var result = await _inner.SendSmsAsync(requestDto);

            if (!result.IsSuccess)
            {
                await _cache.RemoveAsync(key);

                _logger.LogWarning(
                    "SMS failed, lock released for Mobile: {Mobile}",
                    maskedMobile);
            }
            return result;
        }
    }
}