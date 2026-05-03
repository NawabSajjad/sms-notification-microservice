using Npgsql;
using System.Data;

namespace SmsNotification.INFRASTRUCTURE.Data
{
    public class PostgresConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// DI for Connectioon
        /// </summary>
        /// <param name="connectionString"></param>
        public PostgresConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Method for create connection
        /// </summary>
        /// <returns></returns>
        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
