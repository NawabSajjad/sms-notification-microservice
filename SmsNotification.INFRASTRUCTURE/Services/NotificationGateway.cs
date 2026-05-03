using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Utilities;
using SmsNotification.INFRASTRUCTURE.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Web;

public class NotificationGateway : INotificationGateway
{
    private readonly HttpClient _httpClient;
    private readonly NotificationGatewayOptions _options;
    private readonly ILogger<NotificationGateway> _logger;

    /// <summary>
    /// Constructor for NotificationGateway which initializes HttpClient, options and logger. It also validates that the BaseUrl is provided in the options, throwing an exception if it is missing.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public NotificationGateway(
        HttpClient httpClient,
        IOptions<NotificationGatewayOptions> options,
        ILogger<NotificationGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            throw new InvalidOperationException("NotificationGateway BaseUrl missing");

    }
    /// <summary>
    /// Method to send SMS via Notification gateway. It constructs the request URL with URL-encoded parameters, sends a GET request, and logs both the request and response. The method checks for various error conditions in the response and returns a tuple indicating success status and the response content or error message. It also handles exceptions and logs errors if the sending process fails.
    /// </summary>
    /// <param name="mobile"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(bool isSuccess, string response)> SendAsync(
        string mobile,
        string message,
        CancellationToken cancellationToken)
    {
        try
        {
            //  URL encode values
            var url = $"{_options.BaseUrl}?" +
                      $"receiverid={HttpUtility.UrlEncode(mobile)}&" +
                      $"msg={HttpUtility.UrlEncode(message)}&" +
                      $"priority={HttpUtility.UrlEncode(_options.Priority)}";

            _logger.LogInformation(
                "Sending Notification | Mobile: {Mobile} | URL: {Url}",
                MaskingHelper.MaskMobile(mobile),
               MaskingHelper.MaskOtp(url));

            var response = await _httpClient.GetAsync(url, cancellationToken);

            var responseText = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Notification Response | Mobile: {Mobile} | Response: {Response}",
                MaskingHelper.MaskMobile(mobile),
                responseText);

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
            _logger.LogError(ex, "Notification sending failed | Mobile: {Mobile}", MaskingHelper.MaskMobile(mobile));
            return (false, ex.Message);
        }
    }

}