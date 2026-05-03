using StackExchange.Redis;

namespace SmsNotification.INFRASTRUCTURE.Redis
{
    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly IConnectionMultiplexer _connection;

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="connection"></param>
        public RedisConnectionFactory(IConnectionMultiplexer connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Interface  Get Connection
        /// </summary>
        /// <returns></returns>
        public IConnectionMultiplexer GetConnection()
        {
            return _connection;
        }

        /// <summary>
        /// Interface Get database
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }
    }
}
