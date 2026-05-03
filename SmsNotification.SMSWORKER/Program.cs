using SmsNotification.APPLICATION.Interface;
using SmsNotification.INFRASTRUCTURE.Data;
using SmsNotification.INFRASTRUCTURE.Models;
using SmsNotification.INFRASTRUCTURE.Options;
using SmsNotification.INFRASTRUCTURE.Repositories;
using SmsNotification.INFRASTRUCTURE.Services;
using SmsNotification.SMSWORKER;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Security.Authentication;

var builder = Host.CreateApplicationBuilder(args);

//-------------HTTP Client with Polly----------------------------
builder.Services
    .AddHttpClient<INotificationGateway, NotificationGateway>()
    .AddPolicyHandler((serviceProvider, request) =>
    {
        var logger = serviceProvider
            .GetRequiredService<ILogger<NotificationGateway>>();

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(20));

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                3,
                retry => TimeSpan.FromSeconds(Math.Pow(2, retry)),
                (outcome, timespan, retryCount, context) =>
                {
                    logger.LogWarning(
                        outcome.Exception,
                        "Notification Retry {Retry} after {Delay}s",
                        retryCount,
                        timespan.TotalSeconds);
                });

        var circuitPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30),
                (ex, ts) =>
                {
                    logger.LogError("Notification Circuit Open");
                },
                () => logger.LogInformation("Notification Circuit Closed"));

        return Policy.WrapAsync(retryPolicy, circuitPolicy, timeoutPolicy);
    });

// ---------------- RabbitMQ Config ----------------
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

// ---------------- Database Config ----------------
string? Get(string envKey, string configKey) =>
    Environment.GetEnvironmentVariable(envKey)
    ?? builder.Configuration[configKey];

var dbHost = Get("DB_HOST", "ConnectionStrings:PostgresDb:Host");
var dbPort = Get("DB_PORT", "ConnectionStrings:PostgresDb:Port");
var dbName = Get("DB_NAME", "ConnectionStrings:PostgresDb:Database");
var dbUser = Get("DB_USER", "ConnectionStrings:PostgresDb:Username");
var dbPassword = Get("DB_PASSWORD", "ConnectionStrings:PostgresDb:Password");

if (new[] { dbHost, dbPort, dbName, dbUser, dbPassword }
    .Any(string.IsNullOrWhiteSpace))
{
    throw new Exception("Database configuration is missing");
}

var connectionString =
    $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

builder.Services.AddSingleton<IDbConnectionFactory>(
    new PostgresConnectionFactory(connectionString));

builder.Services.AddScoped<ISmsRepository, SmsRepository>();

// ---------------- SMS Gateway (SSL FIX) ----------------
builder.Services.AddHttpClient<ISmsGateway, SmsGateway>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls12,

            //  TEMP FIX FOR SSL ISSUE
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

// ---------------- Gateway Options ----------------
builder.Services.Configure<SmsGatewayOptions>(
    builder.Configuration.GetSection("SmsGateway"));

builder.Services.Configure<NotificationGatewayOptions>(
    builder.Configuration.GetSection("NotificationGateway"));

// ---------------- Worker ----------------
var workerType = Environment.GetEnvironmentVariable("WORKER_TYPE");

if (string.IsNullOrEmpty(workerType))
{
    // Local dev → run both
    builder.Services.AddHostedService<SmsWorkerConsumer>();
    builder.Services.AddHostedService<NotificationWorkerConsumer>();
}
else if (workerType == "sms")
{
    builder.Services.AddHostedService<SmsWorkerConsumer>();
}
else if (workerType == "Notification")
{
    builder.Services.AddHostedService<NotificationWorkerConsumer>();
}
else
{
    throw new Exception("Invalid WORKER_TYPE");
}
var app = builder.Build();
app.Run();