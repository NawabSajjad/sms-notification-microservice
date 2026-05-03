using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Utilities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SmsNotification.INFRASTRUCTURE.Redis
{
    public class RedisTokenBucketRateLimiter : IRateLimiter
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisTokenBucketRateLimiter> _logger;
        private readonly string _luaScript;

        public RedisTokenBucketRateLimiter(
            IRedisConnectionFactory factory,
            ILogger<RedisTokenBucketRateLimiter> logger)
        {
            _db = factory.GetConnection().GetDatabase()
                ?? throw new Exception("Redis DB is null");

            _logger = logger;

            // Load Lua script from file
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "Redis", "Scripts", "token_bucket.lua");

            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Lua script not found: {scriptPath}");

            // Prepare script (uses Redis SHA caching internally)
            _luaScript = File.ReadAllText(scriptPath);
        }

        public async Task<bool> AllowRequestAsync(string key, int capacity, int refillRate)
        {
            try
            {
                var result = (long)await _db.ScriptEvaluateAsync(
                    _luaScript,
                    new RedisKey[] { key },
                    new RedisValue[]
                    {
                capacity,
                refillRate,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    });

                return result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Rate limiter failed for key {Key}, capacity {Capacity}, refill {Refill}",
                    MaskingHelper.MaskMobile(key), capacity, refillRate);

                return true; // fail-open
            }
        }
    }
}
