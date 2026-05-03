using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Messaging;
using SmsNotification.APPLICATION.Models;
using SmsNotification.APPLICATION.Request_DTO;
using SmsNotification.APPLICATION.Utilities;
using SmsNotification.DOMAIN;
using SmsNotification.DOMAIN.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmsNotification.APPLICATION.Service
{
    public class SmsService : ISmsService
    {
        private readonly ISmsRepository _repostiory;
        private readonly ISmsTemplateRepository _smsTemplateRepository;
        private readonly ILogger<SmsService> _logger;
        public readonly IRabbitMqPublisher _publisher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="repostiory"></param>
        /// <param name="smsGateway"></param>
        /// <param name="smsTemplateRepository"></param>
        public SmsService(
            ISmsRepository repostiory,
            ISmsTemplateRepository smsTemplateRepository,
            IRabbitMqPublisher publisher,
            ILogger<SmsService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _repostiory = repostiory;
            _smsTemplateRepository = smsTemplateRepository;
            _publisher = publisher;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Method of send sms
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        public async Task<OtpSendResult> SendSmsAsync(SmsRequestDto requestDto)
        {
            var correlationId = _httpContextAccessor.HttpContext?
                .Items["CorrelationId"]?.ToString()
                ?? Guid.NewGuid().ToString();

            _logger.LogInformation(
                "OTP request initiated. Mobile: {Mobile}, Channels: {Channels}, CorrelationId={CorrelationId}",
                MaskingHelper.MaskMobile(requestDto.mobile),
                string.Join(",", requestDto.channels),
                correlationId);

            try
            {
                var sms = new Sms(
                    requestDto.otp,
                    requestDto.mobile,
                    requestDto.NotificationStatus,
                    requestDto.smsStatus
                );

                var auditMeta = new SmsAuditInfo
                {
                    ClientIP = requestDto.clientIP,
                    Flag = requestDto.flag,
                    CountryMobileCode = requestDto.countryMobileCode,
                    UserName = requestDto.userName,
                    PageName = requestDto.pageName
                };

                var (smsId, result) = await _repostiory.InsertSmsAsync(sms, auditMeta);

                if (result == "OTP_ALREADY_SENT")
                {
                    return OtpSendResult.AlreadySent();
                }

                if (result != "OTP_SENT")
                {
                    return OtpSendResult.Failed();
                }

                var template = await _smsTemplateRepository
                    .GetByTemplateNameAsync(requestDto.templateName, CancellationToken.None)
                    ?? throw new BusinessException("SMS template not found");

                var message = SmsTemplateRenderer.Render(
                    template.SmsContent,
                    new Dictionary<string, string>
                    {
                        ["var"] = requestDto.otp
                    });

           
                var queueMessage = new SmsQueueMessage(
                    smsId: smsId,
                    mobile: requestDto.mobile,
                    message: message,
                    templateId: template.TemplateDltId,
                    correlationId: correlationId,
                    NotificationStatus: requestDto.NotificationStatus,
                    channel: string.Join(",", requestDto.channels)
                );

                var isPublished = await _publisher.PublishAsync(queueMessage);

                if (!isPublished)
                {
                    return OtpSendResult.Failed();
                }

                var channels = requestDto.channels.Select(x => x.ToLower()).ToList();

                if (channels.Contains("sms") && channels.Contains("Notification"))
                {
                    return OtpSendResult.Queued("both");
                }
                else if (channels.Contains("sms"))
                {
                    return OtpSendResult.Queued("sms");
                }
                else if (channels.Contains("Notification"))
                {
                    return OtpSendResult.Queued("Notification");
                }
                else
                {
                    return OtpSendResult.Failed();
                }
            }
            catch (BusinessException ex)
            {
                _logger.LogError(ex, "Business exception occurred while sending SMS to {Mobile}, CorrelationId={CorrelationId}",
                        MaskingHelper.MaskMobile(requestDto.mobile), correlationId);
                throw;

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error occurred while sending SMS to {Mobile},CorrelationId={CorrelationId}",
                    MaskingHelper.MaskMobile(requestDto.mobile), correlationId);
                throw;
            }
           
        }

    }
}
