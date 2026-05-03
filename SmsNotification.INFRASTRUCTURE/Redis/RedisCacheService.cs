using SmsNotification.APPLICATION.Interface;
using StackExchange.Redis;
using System.Text.Json;

namespace SmsNotification.INFRASTRUCTURE.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="factory"></param>
        public RedisCacheService(IRedisConnectionFactory factory)
        {
            _db = factory.GetDatabase();
        }

        /// <summary>
        /// method of set async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task SetAsync<T>(string key, T value, TimeSpan expiry)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry);
        }

        /// <summary>
        /// method of get async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        /// <summary>
        /// entity of remove async
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        /// <summary>
        /// entity exist async
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }

        /// <summary>
        /// entity of set expiry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan expiry)
        {
            return await _db.StringSetAsync(
                key,
                value,
                expiry,
                When.NotExists
            );
        }
    }
}
