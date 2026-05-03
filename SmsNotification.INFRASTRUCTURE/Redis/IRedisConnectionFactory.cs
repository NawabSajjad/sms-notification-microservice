using StackExchange.Redis;

namespace SmsNotification.INFRASTRUCTURE.Redis
{   
    public interface IRedisConnectionFactory
    {
        /// <summary>
        /// Returns the Redis connection multiplexer
        /// </summary>
        IConnectionMultiplexer GetConnection();

        /// <summary>
        /// Returns Redis database instance
        /// </summary>
        IDatabase GetDatabase();
    }
}
