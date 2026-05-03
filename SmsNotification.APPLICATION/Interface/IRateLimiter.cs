namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for request rate limiting.
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Determines whether a request is allowed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="capacity"></param>
        /// <param name="refillRate"></param>
        /// <returns></returns>
        Task<bool> AllowRequestAsync(
            string key,
            int capacity,
            int refillRate);
    }
}