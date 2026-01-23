using DGC.eKYC.Business.DTOs.RedisQueueMessage;
using Microsoft.Extensions.Logging;

namespace DGC.eKYC.Business.Services.RedisMessageQueue;

public interface IRedisMessageQueue<T>
{
    Task EnqueueAsync(T message, bool fireAndForget);
    Task<BaseRedisQueueMessage<T>?> DequeueAsync(int retryDelay, CancellationToken cancellationToken);
    Task RequeueAsync(BaseRedisQueueMessage<T> message);
    Task MoveToDeadLetterAsync(BaseRedisQueueMessage<T> message);

    Task HandleProcessingErrorAsync(
        IRedisMessageQueue<T> queue,
        BaseRedisQueueMessage<T> envelope,
        int retryLimit,
        int retryDelayMs,
        ILogger logger,
        Exception ex,
        CancellationToken stoppingToken);
}