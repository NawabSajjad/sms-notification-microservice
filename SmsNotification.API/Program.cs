using SmsNotification.APPLICATION.Decorators;
using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Service;
using SmsNotification.INFRASTRUCTURE.Data;
using SmsNotification.INFRASTRUCTURE.Models;
using SmsNotification.INFRASTRUCTURE.Options;
using SmsNotification.INFRASTRUCTURE.Redis;
using SmsNotification.INFRASTRUCTURE.Repositories;
using SmsNotification.INFRASTRUCTURE.Services;
using SmsNotification.SMS.API.Middleware;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

#region Controllers & Swagger
// Register controllers and Swagger services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region Application Services
// Register SMS services and decorators
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.Decorate<ISmsService, SmsLoggingDecorator>();
builder.Services.Decorate<ISmsService, SmsCacheDecorator>();
builder.Services.Decorate<ISmsService, SmsValidationDecorator>();

// Register repositories and infrastructure services
builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
builder.Services.AddScoped<IErrorLogger, ErrorLoggerService>();
builder.Services.AddScoped<ISmsTemplateRepository, SmsTemplateRepository>();
builder.Services.AddScoped<ISmsRepository, SmsRepository>();
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
#endregion

#region RabbitMQ Config
// Bind RabbitMQ configuration from appsettings
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
#endregion

#region HTTP Client with Polly
// Configure HTTP client with retry, timeout, and circuit breaker policies
builder.Services
    .AddHttpClient<ISmsGateway, SmsGateway>()
    .AddPolicyHandler((serviceProvider, request) =>
    {
        var logger = serviceProvider.GetRequiredService<ILogger<SmsGateway>>();

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(20),
            TimeoutStrategy.Pessimistic,
            onTimeoutAsync: (context, timespan, task, exception) =>
            {
                logger.LogError(
                    "Timeout after {TimeoutSeconds}s",
                    timespan.TotalSeconds);

                return Task.CompletedTask;
            });

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                3,
                retry => TimeSpan.FromSeconds(Math.Pow(2, retry)),
                (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning(
                        outcome.Exception,
                        "Retry {RetryAttempt} after {Delay}s",
                        retryAttempt,
                        timespan.TotalSeconds);
                });

        var circuitPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30),
                (outcome, breakDelay) =>
                {
                    logger.LogError(
                        outcome.Exception,
                        "Circuit opened for {BreakDelay}s",
                        breakDelay.TotalSeconds);
                },
                () => logger.LogInformation("Circuit closed"),
                () => logger.LogWarning("Circuit half-open"));

        return Policy.WrapAsync(
            timeoutPolicy,
            retryPolicy,
            circuitPolicy);
    });
#endregion

#region Database Config
// Read database configuration from environment variables or appsettings
string? Get(string envKey, string configKey) =>
    Environment.GetEnvironmentVariable(envKey)
    ?? builder.Configuration[configKey];

var dbHost = Get("DB_HOST", "ConnectionStrings:PostgresDb:Host");
var dbPort = Get("DB_PORT", "ConnectionStrings:PostgresDb:Port");
var dbName = Get("DB_NAME", "ConnectionStrings:PostgresDb:Database");
var dbUser = Get("DB_USER", "ConnectionStrings:PostgresDb:Username");
var dbPassword = Get("DB_PASSWORD", "ConnectionStrings:PostgresDb:Password");

// Validate database configuration values
if (new[] { dbHost, dbPort, dbName, dbUser, dbPassword }
    .Any(string.IsNullOrWhiteSpace))
{
    throw new Exception("Database configuration is missing");
}

// Build PostgreSQL connection string
var connectionString =
$"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};SSL Mode=Disable;Timeout=15;CommandTimeout=30;Pooling=true;Maximum Pool Size=100";

// Register database connection factory
builder.Services.AddSingleton<IDbConnectionFactory>(
    new PostgresConnectionFactory(connectionString));
#endregion

#region Redis Config
// Read Redis connection string
var redisConnection = builder.Configuration["Redis:ConnectionString"];

// Configure Redis connection settings
var redisOptions = ConfigurationOptions.Parse(redisConnection!);
redisOptions.AbortOnConnectFail = false;
redisOptions.ConnectRetry = 3;
redisOptions.ConnectTimeout = 5000;
redisOptions.SyncTimeout = 5000;
redisOptions.AsyncTimeout = 5000;
redisOptions.KeepAlive = 60;
redisOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);

// Register Redis connection multiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisOptions));

// Register Redis connection factory
builder.Services.AddSingleton<IRedisConnectionFactory>(serviceProvider =>
{
    var multiplexer =
        serviceProvider.GetRequiredService<IConnectionMultiplexer>();

    return new RedisConnectionFactory(multiplexer);
});

// Register Redis cache and rate limiter services
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddSingleton<IRateLimiter, RedisTokenBucketRateLimiter>();
#endregion

#region Other Config
// Bind SMS gateway configuration
builder.Services.Configure<SmsGatewayOptions>(
    builder.Configuration.GetSection("SmsGateway"));

// Register HTTP context accessor
builder.Services.AddHttpContextAccessor();
#endregion

var app = builder.Build();

#region Middleware
// Enable Swagger only in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Register global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable authorization middleware
app.UseAuthorization();
#endregion

#region Endpoints
// Map controller endpoints
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok("Healthy"));
#endregion

app.Run();