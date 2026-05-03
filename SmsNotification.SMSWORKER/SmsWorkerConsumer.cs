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
    /// Background service that consumes SMS messages from a RabbitMQ queue,
    /// sends them via the configured SMS gateway, and updates their status in the database.
    /// </summary>
    public class SmsWorkerConsumer : BackgroundService
    {
        private readonly ISmsGateway _smsGateway;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _config;
        private readonly ILogger<SmsWorkerConsumer> _logger;

        private IConnection _connection = default!;
        private IModel _channel = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsWorkerConsumer"/> class.
        /// </summary>
        public SmsWorkerConsumer(
            ISmsGateway smsGateway,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<SmsWorkerConsumer> logger)
        {
            _smsGateway = smsGateway;
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
        /// Establishes a connection to RabbitMQ with retry logic and sets up event handlers for connection issues.
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
                        "Worker starting RabbitMQ connection. Host={Host} Queue={Queue}",
                        _config.Host,
                        _config.SmsQueue);

                    _connection = factory.CreateConnection();

                    // Log connection shutdown events.
                    _connection.ConnectionShutdown += (sender, eventArgs) =>
                    {
                        _logger.LogWarning(
                            "RabbitMQ connection shutdown. ReplyCode={Code}, ReplyText={Text}",
                            eventArgs.ReplyCode,
                            eventArgs.ReplyText);
                    };

                    // Log callback exceptions.
                    _connection.CallbackException += (sender, eventArgs) =>
                    {
                        _logger.LogError(eventArgs.Exception,
                            "RabbitMQ callback exception occurred.");
                    };

                    _channel = _connection.CreateModel();

                    _logger.LogInformation("RabbitMQ connection established successfully.");

                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "RabbitMQ connection failed. Retrying in 10 seconds.");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        /// <summary>
        /// Declares the necessary RabbitMQ queues and exchange for SMS message processing.
        /// </summary>
        private void DeclareQueues()
        {
            _logger.LogInformation(
             "Declaring queues: {Queue} with Exchange: {Exchange}",
             _config.SmsQueue,
             _config.ExchangeName);

            var args = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", "" },
                    { "x-dead-letter-routing-key",_config.SmsDlq }
                };

            if (string.IsNullOrWhiteSpace(_config.ExchangeName))
            {
                throw new Exception("ExchangeName is not configured in appsettings");
            }

            // Declare the main exchange for SMS messages.
            _channel.ExchangeDeclare(
                exchange: _config.ExchangeName,
                type: ExchangeType.Direct,
                durable: true);

            // Declare the main SMS queue with dead-letter configuration.
            _channel.QueueDeclare(
                queue: _config.SmsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: args);

            // Bind the SMS queue to the exchange with the "sms" routing key.
            _channel.QueueBind(
                 queue: _config.SmsQueue,
                 exchange: _config.ExchangeName,
                 routingKey: "sms");

            // Declare the dead-letter queue.
            _channel.QueueDeclare(
                queue: _config.SmsDlq,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Set the prefetch count for fair dispatch.
            _channel.BasicQos(0, 10, false);

            _logger.LogInformation("Queue declaration completed.");
        }

        /// <summary>
        /// Starts consuming messages from the SMS queue and processes each message.
        /// </summary>
        private void StartConsumer()
        {
            _logger.LogInformation("Worker started consuming queue {Queue}", _config.SmsQueue);

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
                        _logger.LogWarning("Received null message from queue.");
                        _channel.BasicReject(ea.DeliveryTag, false);
                        return;
                    }

                    _logger.LogInformation(
                        "Message received. SmsId={SmsId}, Mobile={Mobile}, CorrelationId={CorrelationId}",
                        message.smsId,
                        MaskingHelper.MaskMobile(message.mobile), message.correlationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message deserialization failed.");
                    _channel.BasicReject(ea.DeliveryTag, false);
                    return;
                }

                // Create a new scope for repository usage.
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISmsRepository>();

                try
                {
                    _logger.LogInformation(
                        "Sending SMS to gateway. SmsId={SmsId}, CorrelationId={CorrelationId}",
                        message.smsId, message.correlationId);

                    // Send the SMS using the gateway.
                    var result = await _smsGateway.SendAsync(
                        message.mobile,
                        message.message,
                        message.templateId,
                        CancellationToken.None);

                    _logger.LogInformation(
                        "Gateway response received. SmsId={SmsId}, Success={Success}, CorrelationId={CorrelationId} ",
                        message.smsId,
                        result.isSuccess,
                        message.correlationId);

                    // Update the message status in the database.
                    await repository.UpdateSmsStatusAsync(
                        message.smsId,
                        result.isSuccess ? 1 : -1,
                        result.response);

                    _logger.LogInformation(
                        "Database status updated. SmsId={SmsId}, Status={Status}, CorrelationId={CorrelationId}",
                        message.smsId,
                        result.isSuccess ? "SUCCESS" : "FAILED",
                        message.correlationId);

                    // Acknowledge the message as processed.
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (TimeoutRejectedException ex)
                {
                    _logger.LogWarning(ex,
                        "SMS gateway timeout. SmsId={SmsId}, CorrelationId={CorrelationId}",
                        message.smsId,
                        message.correlationId);

                    await repository.UpdateSmsStatusAsync(
                        message.smsId,
                        -1,
                        "Timeout");

                    // Reject the message so it can be dead-lettered.
                    _channel.BasicReject(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "SMS processing failed. SmsId={SmsId}, CorrelationId={CorrelationId}",
                        message.smsId,
                        message.correlationId);

                    await repository.UpdateSmsStatusAsync(
                        message.smsId,
                        -1,
                        ex.Message);

                    // Reject the message so it can be dead-lettered.
                    _channel.BasicReject(ea.DeliveryTag, false);
                }
            };

            // Start consuming messages from the queue.
            _channel.BasicConsume(
                queue: _config.SmsQueue,
                autoAck: false,
                consumer: consumer);
        }

        /// <summary>
        /// Disposes the RabbitMQ channel and connection when the service is stopped.
        /// </summary>
        public override void Dispose()
        {
            try
            {
                if (_channel?.IsOpen == true)
                    _channel.Close();

                if (_connection?.IsOpen == true)
                    _connection.Close();

                _logger.LogInformation("RabbitMQ connection disposed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection.");
            }

            base.Dispose();
        }
    }
}