using System.Data;

namespace SmsNotification.INFRASTRUCTURE.Data
{
    /// <summary>
    /// Interface for DB connection
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates and returns a new database connection instance.
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
    }
}
