using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Utilities;
using SmsNotification.DOMAIN;
using SmsNotification.INFRASTRUCTURE.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmsNotification.INFRASTRUCTURE.Services
{
    public class SmsGateway : ISmsGateway
    {
        private readonly HttpClient _httpClient;
        private readonly SmsGatewayOptions _options;
        private readonly ILogger<SmsGateway> _logger;

        /// <summary>
        /// Method for constructor of class which initializes HttpClient, options and logger. It also validates that the BaseUrl and UserName are provided in the options, throwing exceptions if they are missing.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public SmsGateway(
                     HttpClient httpClient,
                     IOptions<SmsGatewayOptions> options,
                     ILogger<SmsGateway> logger)
        {
            _httpClient = httpClient;
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
                throw new InvalidOperationException("SmsGateway BaseUrl missing");

            if (string.IsNullOrWhiteSpace(_options.UserName))
                throw new InvalidOperationException("SmsGateway UserName missing");
        }

        /// <summary>
        /// Method to send SMS via the gateway. It constructs the request data, sends a POST request, and logs both the request and response. The method checks for various error conditions in the response and returns a tuple indicating success status and the response content or error message. It also handles exceptions and logs errors if the sending process fails.
        /// </summary>
        /// <param name="mobileNo"></param>
        /// <param name="message"></param>
        /// <param name="templateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<(bool isSuccess, string response)> SendAsync(
            string mobileNo,
            string message,
            string templateId,
            CancellationToken cancellationToken)
        {

            try
            {

                var data = new Dictionary<string, string>
                {
                    ["username"] = _options.UserName,
                    ["pin"] = _options.Pin,
                    ["message"] = message,
                    ["mnumber"] = mobileNo,
                    ["signature"] = _options.SenderId,
                    ["dlt_entity_id"] = _options.EntityId,
                    ["dlt_template_id"] = templateId
                }
                ;

                if (message.Length > 160)
                    data.Add("concat", "1");

                _logger.LogInformation(
                 "Sending SMS | Mobile: {Mobile} | Template: {TemplateId} | Message: {Message}",
                 MaskingHelper.MaskMobile(mobileNo), templateId, MaskingHelper.MaskOtp(message));

                using var content = new FormUrlEncodedContent(data);

                var response = await _httpClient.PostAsync(
                    _options.BaseUrl,
                    content,
                    cancellationToken);

                var responseText = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                     "SMS Gateway Response | Mobile: {Mobile} | Response: {Response}",
                     MaskingHelper.MaskMobile(mobileNo), responseText);


                if (!response.IsSuccessStatusCode ||
                       responseText.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                       responseText.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                       responseText.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
                       responseText.Contains("rejected", StringComparison.OrdinalIgnoreCase) ||
                       responseText.Contains("failed", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, responseText);
                }

                return (true, responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SMS Sending Failed | Mobile: {Mobile}",
                     MaskingHelper.MaskMobile(mobileNo));

                return (false, ex.ToString());
            }
        }
    }
}

