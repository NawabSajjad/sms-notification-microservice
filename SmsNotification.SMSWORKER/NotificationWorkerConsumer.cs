using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Messaging;
using SmsNotification.APPLICATION.Utilities;
using SmsNotification.INFRASTRUCTURE.Options;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace SmsNotification.SMSWORKER
{
    /// <summary>
    /// Background service that consumes messages from a RabbitMQ queue and processes them using the Notification gateway.
    /// Handles message deserialization, sending via Notification, and updating message status in the repository.
    /// </summary>
    public class NotificationWorkerConsumer : BackgroundService
    {
        private readonly INotificationGateway _NotificationGateway;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _config;
        private readonly ILogger<NotificationWorkerConsumer> _logger;

        private IConnection _connection = default!;
        private IModel _channel = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationWorkerConsumer"/> class.
        /// </summary>
        public NotificationWorkerConsumer(
            INotificationGateway NotifactionGateway,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<NotificationWorkerConsumer> logger)
        {
            _NotificationGateway = NotifactionGateway;
            _scopeFactory = scopeFactory;
            _config = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Main execution loop for the background service.
        /// Connects to RabbitMQ, declares queues, and starts consuming messages.
        /// </summary>
        /// <param name="stoppingToken">Token to signal service stop.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConnectRabbitMq(stoppingToken);
            DeclareQueues();
            StartConsumer();

            // Keep the service running until cancellation is requested.
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        /// <summary>
        /// Establishes a connection to RabbitMQ with retry logic.
        /// </summary>
        /// <param name="stoppingToken">Token to signal service stop.</param>
        private async Task ConnectRabbitMq(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _config.Host,
                Port = _config.Port,
                UserName = _config.Username,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                TopologyRecoveryEnabled = true
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation(
                        "Notification Worker connecting to RabbitMQ. Queue={Queue}",
                        _config.NotificationQueue);

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _logger.LogInformation("RabbitMQ connection established.");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ connection failed. Retrying...");
                    await Task.Delay(10000, stoppingToken);
                }
            }
        }

        /// <summary>
        /// Declares the necessary RabbitMQ queues and exchange for Notification message processing.
        /// </summary>
        private void DeclareQueues()
        {
            if (_channel == null)
                throw new Exception("RabbitMQ channel not initialized");

            var args = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", "" },
                    { "x-dead-letter-routing-key", _config.NotificationDlq }
                };

            _channel.ExchangeDeclare(
                exchange: _config.ExchangeName,
                type: ExchangeType.Direct,
                durable: true);

            _channel.QueueDeclare(
                queue: _config.NotificationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: args);

            _channel.QueueBind(
                queue: _config.NotificationQueue,
                exchange: _config.ExchangeName,
                routingKey: _config.NotificationRoutingKey);

            _channel.QueueDeclare(
                queue: _config.NotificationDlq,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.BasicQos(0, 10, false);

            _logger.LogInformation("Notification queue declared successfully.");
        }

        /// <summary>
        /// Starts consuming messages from the Notification queue and processes each message.
        /// </summary>
        private void StartConsumer()
        {
            if (_channel == null)
                throw new Exception("RabbitMQ channel not initialized");

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                SmsQueueMessage? message = null;

                try
                {
                    // Deserialize the message from the queue.
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    message = JsonSerializer.Deserialize<SmsQueueMessage>(json);

                    if (message == null)
                    {
                        _logger.LogWarning("Received null message.");
                        _channel.BasicReject(ea.DeliveryTag, false);
                        return;
                    }

                    _logger.LogInformation(
                        "[Notification] Message received. SmsId={SmsId}, Mobile={Mobile}, CorrelationId={CorrelationId}",
                        message.smsId,
                        MaskingHelper.MaskMobile(message.mobile),
                        message.correlationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Deserialization failed.");
                    _channel.BasicReject(ea.DeliveryTag, false);
                    return;
                }

                // Create a new scope for repository usage.
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISmsRepository>();

                try
                {
                    // Send the message using the Notification gateway.
                    var result = await _NotificationGateway.SendAsync(
                        message.mobile,
                        message.message,
                        CancellationToken.None);

                    // Update the message status in the repository.
                    await repository.UpdateNotificationStatusAsync(
                        message.smsId,
                        result.isSuccess ? 1 : -1,
                        result.response);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (TimeoutRejectedException ex)
                {
                    _logger.LogWarning(ex, "Notification timeout. SmsId={SmsId}", message!.smsId);

                    await repository.UpdateNotificationStatusAsync(
                        message!.smsId,
                        -1,
                        "Timeout");

                    _channel.BasicReject(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Notification processing failed. SmsId={SmsId}", message!.smsId);

                    await repository.UpdateNotificationStatusAsync(
                        message!.smsId,
                        -1,
                        ex.Message);

                    _channel.BasicReject(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(
                queue: _config.NotificationQueue,
                autoAck: false,
                consumer: consumer);
        }
    }
}