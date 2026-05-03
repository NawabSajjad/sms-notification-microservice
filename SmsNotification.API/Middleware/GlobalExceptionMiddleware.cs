using SmsNotification.APPLICATION.Interface;
using SmsNotification.DOMAIN;
using System.Net;

namespace SmsNotification.SMS.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        /// <summary>
        /// Initializes GlobalExceptionMiddleware dependencies.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Handles application exceptions asynchronously.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessException ex)
            {
                await HandleException(
                    context,
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR",
                    ex.Message,
                    ex,
                    LogLevel.Warning);
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleException(
                    context,
                    HttpStatusCode.Unauthorized,
                    "UNAUTHORIZED",
                    "Authentication is required",
                    ex,
                    LogLevel.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                var errorLogger =
                    context.RequestServices.GetRequiredService<IErrorLogger>();

                var source = GetSource(context);

                if (IsTimeout(ex))
                {
                    _ = Task.Run(() =>
                        errorLogger.LogAsync(
                            source: source,
                            errorCode: "TIMEOUT",
                             httpStatusCode: StatusCodes.Status504GatewayTimeout,
                            exception: ex)
                    );

                    await HandleException(
                        context,
                        HttpStatusCode.GatewayTimeout,
                        "TIMEOUT",
                        "Request timed out. Please try again.",
                        ex,
                        LogLevel.Warning);

                    return;
                }

                _ = Task.Run(() =>
                    errorLogger.LogAsync(
                        source: source,
                        errorCode: "INTERNAL_ERROR",
                        httpStatusCode: StatusCodes.Status500InternalServerError,
                        exception: ex)
                );

                await HandleException(
                    context,
                    HttpStatusCode.InternalServerError,
                    "INTERNAL_ERROR",
                    "An unexpected error occurred",
                    ex,
                    LogLevel.Error);
            }
        }

        /// <summary>
        /// Writes standardized error response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statusCode"></param>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        private async Task HandleException(
            HttpContext context,
            HttpStatusCode statusCode,
            string code,
            string message,
            Exception exception,
            LogLevel logLevel)
        {
            _logger.Log(logLevel, exception, "Request failed: {Code}", code);

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                code,
                message
            });
        }

        /// <summary>
        /// Checks whether the exception is timeout related.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static bool IsTimeout(Exception ex)
        {
            return ex is TimeoutException
                || ex is TaskCanceledException
                || ex.InnerException is TimeoutException;
        }

        /// <summary>
        /// Retrieves request source information.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string GetSource(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
                return "UNKNOWN_ENDPOINT";

            var routeData = endpoint.Metadata
                .GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();

            if (routeData == null)
                return "UNKNOWN_ENDPOINT";

            return $"{routeData.ControllerName}.{routeData.ActionName}";
        }
    }
}
