using SmsNotification.APPLICATION.Interface;
using SmsNotification.DOMAIN.Entities;
using SmsNotification.INFRASTRUCTURE.Data;
using Dapper;
using System.Data;

namespace SmsNotification.INFRASTRUCTURE.Repositories
{
    public class SmsRepository : ISmsRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        /// <summary>
        /// DI for Connection
        /// </summary>
        /// <param name="dbConnectionFactory"></param>
        public SmsRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        /// <summary>
        /// Methode for insert data into DB
        /// </summary>
        /// <param name="sms"></param>
        /// <param name="smsMeta"></param>
        /// <returns></returns>
        public async Task<(long smsId, string result)> InsertSmsAsync(Sms sms, SmsAuditInfo smsMeta)
        {
            using var conn = _dbConnectionFactory.CreateConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_otp", sms.Otp);
                parameters.Add("p_mobile", sms.Mobile);
                parameters.Add("p_clientip", smsMeta.ClientIP);
                parameters.Add("p_flag", smsMeta.Flag);
                parameters.Add("p_pagename", smsMeta.PageName);
                parameters.Add("p_countrymobilecode", smsMeta.CountryMobileCode);
                parameters.Add("p_username", smsMeta.UserName);
                parameters.Add("p_Notificationstatus", sms.NotificationStatus);
                parameters.Add("p_smsstatus", sms.SmsStatus);
                parameters.Add("p_Notificationid", dbType: DbType.Int64, direction: ParameterDirection.InputOutput);
                parameters.Add("p_result", dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 4000);

                await conn.ExecuteAsync(
               "CALL procinsertloginotpNotification(@p_otp,@p_mobile,@p_clientip,@p_flag,@p_pagename,@p_countrymobilecode,@p_username,@p_Notificationstatus,@p_smsstatus,@p_Notificationid,@p_result)",
                 parameters,
                 transaction
                );

                transaction.Commit();
                return (
                parameters.Get<long?>("p_Notificationid") ?? 0,
                parameters.Get<string>("p_result")
         );
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

        }

        /// <summary>
        /// Update Notification status
        /// </summary>
        /// <param name="smsId"></param>
        /// <param name="status"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> UpdateNotificationStatusAsync(long smsId, int status, string response)
        {
            using var conn = _dbConnectionFactory.CreateConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("p_Notificationid", smsId);
                parameters.Add("p_status", status);
                parameters.Add("p_response", response);
                parameters.Add("p_result", dbType: DbType.String,
                               direction: ParameterDirection.InputOutput,
                size: 5000);

                await conn.ExecuteAsync(
                    "CALL proc_update_Notification_status(@p_Notificationid,@p_status,@p_response,@p_result)",
                    parameters,
                    transaction
                    );

                transaction.Commit();
                return parameters.Get<string>("p_result");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        

        /// <summary>
        /// Update status
        /// </summary>
        /// <param name="smsId"></param>
        /// <param name="status"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<string> UpdateSmsStatusAsync(long smsId, int status, string response)
        {

            using var conn = _dbConnectionFactory.CreateConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {

                var parameters = new DynamicParameters();

                parameters.Add("p_Notificationid", smsId);
                parameters.Add("p_status", status);
                parameters.Add("p_response", response);
                parameters.Add("p_result", dbType: DbType.String,
                               direction: ParameterDirection.InputOutput,
                size: 5000);

                await conn.ExecuteAsync(
                    "CALL proc_update_sms_status(@p_Notificationid,@p_status,@p_response,@p_result)",
                    parameters,
                    transaction
                    );

                transaction.Commit();
                return parameters.Get<string>("p_result");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
