using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DGC.eKYC.Business.Services.RedisMessageQueue;

public abstract class BaseBackgroundService<T>(IServiceScopeFactory serviceScopeFactory, ILogger logger) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var queue = scope.ServiceProvider.GetRequiredService<IRedisMessageQueue<T>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var retryLimit = configuration.GetValue("RedisQueue:RedisMessageProcessor:RetryLimit", 10);
            var retryDelayMs = configuration.GetValue("RedisQueue:RedisMessageProcessor:RetryDelayMs", 100);

            var envelope = await queue.DequeueAsync(retryDelayMs, stoppingToken);
            if (envelope == null) continue;

            try
            {
                await ProcessMessageAsync(scope.ServiceProvider, envelope.Payload, stoppingToken);
            }
            catch (Exception ex)
            {
                await queue.HandleProcessingErrorAsync(
                    queue,
                    envelope,
                    retryLimit,
                    retryDelayMs,
                    _logger,
                    ex,
                    stoppingToken);
            }
        }
    }

    protected abstract Task ProcessMessageAsync(IServiceProvider serviceProvider, T message, CancellationToken token);
}