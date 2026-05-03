using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace SmsNotification.INFRASTRUCTURE.Resilience
{
    public static class PollyPolicies
    {
        /// <summary>
        /// Method to get retry policy for HttpClient with exponential backoff strategy and logging of retry attempts.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
            ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogWarning(
                            "Retry {RetryAttempt} after {Delay}s",
                            retryAttempt,
                            timespan.TotalSeconds);
                    });
        }
    }
}
