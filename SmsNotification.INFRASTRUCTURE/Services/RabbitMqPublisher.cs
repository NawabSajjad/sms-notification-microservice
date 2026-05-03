using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Messaging;
using SmsNotification.INFRASTRUCTURE.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace SmsNotification.INFRASTRUCTURE.Services
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly RabbitMqOptions _config;
        private readonly ILogger<RabbitMqPublisher> _logger;

        private IConnection? _connection;
        private IModel? _channel;

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public RabbitMqPublisher(IOptions<RabbitMqOptions> options,
                                 ILogger<RabbitMqPublisher> logger)
        {
            _config = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Method to ensure RabbitMQ connection is established before publishing messages. It implements a retry mechanism with logging for connection attempts and failures.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void EnsureConnection()
        {
            if (_connection != null && _connection.IsOpen) return;


            var factory = new ConnectionFactory()
            {
                HostName = _config.Host,
                Port = _config.Port,
                UserName = _config.Username,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
                Ssl = { Enabled = false }
            };

            int retry = 0;

            while (retry < 10)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _logger.LogInformation("RabbitMQ connected successfully");
                    return;
                }
                catch (Exception ex)
                {
                    retry++;
                    _logger.LogWarning(ex, "RabbitMQ connection failed. Retry {Retry}/10", retry);

                    Thread.Sleep(5000); // wait 5 sec
                }
            }

            _logger.LogError("RabbitMQ connection failed after retries");
            throw new Exception("Unable to connect to RabbitMQ");

        }
        /// <summary>
        /// Method to publish messages to RabbitMQ. It checks the message channel to determine whether to send to SMS or Notification queues, and logs the publishing process with correlation IDs for traceability. It also handles exceptions and logs errors if publishing fails.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<bool> PublishAsync(SmsQueueMessage message)
        {
            try
            {
                EnsureConnection();

                if (_channel == null)
                {
                    _logger.LogWarning("RabbitMQ not connected");
                    return Task.FromResult(false);
                }

                _channel.ExchangeDeclare(
                    exchange: _config.ExchangeName,
                    type: ExchangeType.Direct,
                    durable: true);

                var body = Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(message));

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                var channel = message.channel?.ToLower() ?? "";

                // SEND SMS ONLY IF REQUIRED
                if (channel.Contains("sms"))
                {
                    _channel.BasicPublish(
                        exchange: _config.ExchangeName,
                        routingKey: _config.SmsRoutingKey,
                        basicProperties: properties,
                        body: body);

                    _logger.LogInformation("Published to SMS queue. CorrelationId={CorrelationId}",
                        message.correlationId);
                }

                // SEND Notification ONLY IF REQUIRED
                if (channel.Contains("Notification"))
                {
                    _channel.BasicPublish(
                        exchange: _config.ExchangeName,
                        routingKey: _config.NotificationRoutingKey,
                        basicProperties: properties,
                        body: body);

                    _logger.LogInformation("Published to Notification queue. CorrelationId={CorrelationId}",
                        message.correlationId);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Publish failed CorrelationId={CorrelationId}",
                    message.correlationId);

                return Task.FromResult(false);
            }
        }

    }
}
