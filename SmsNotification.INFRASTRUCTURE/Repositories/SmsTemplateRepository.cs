using SmsNotification.APPLICATION.Interface;
using SmsNotification.DOMAIN.Entities;
using SmsNotification.INFRASTRUCTURE.Data;
using Dapper;

namespace SmsNotification.INFRASTRUCTURE.Repositories
{
    public class SmsTemplateRepository : ISmsTemplateRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        /// <summary>
        /// DI for connection
        /// </summary>
        /// <param name="dbConnectionFactory"></param>
        public SmsTemplateRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        /// <summary>
        /// Method to call function for template
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SmsTemplate?> GetByTemplateNameAsync(
        string templateName,
        CancellationToken cancellationToken)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("p_template_name", templateName);

            return await connection.QueryFirstOrDefaultAsync<SmsTemplate>(
                new CommandDefinition(
                   "SELECT * FROM fn_get_sms_template_by_name(@p_template_name)",
                    new { p_template_name = templateName },
                    cancellationToken: cancellationToken
                ));
        }
    }
}
