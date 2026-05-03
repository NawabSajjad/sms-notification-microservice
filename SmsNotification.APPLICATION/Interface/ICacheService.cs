namespace SmsNotification.APPLICATION.Interface
{
    /// <summary>
    /// Defines methods for cache operations such as set, get,
    /// remove, existence check, and conditional insert.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Asynchronously sets a value in the cache with an expiration time.
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan expiry);

        /// <summary>
        /// Asynchronously retrieves a value from the cache by key.
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Asynchronously removes a value from the cache by key.
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Asynchronously checks if a key exists in the cache.
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Asynchronously sets a value in the cache only if the key does not already exist.
        /// </summary>
        Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan expiry);
    }
}
