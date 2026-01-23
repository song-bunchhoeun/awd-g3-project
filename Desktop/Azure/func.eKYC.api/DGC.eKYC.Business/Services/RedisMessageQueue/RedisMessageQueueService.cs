using System.Text.Json;
using DGC.eKYC.Business.DTOs.RedisQueueMessage;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DGC.eKYC.Business.Services.RedisMessageQueue;

public class RedisMessageQueueService<T>(
    IConnectionMultiplexer redis,
    string queueName,
    JsonSerializerOptions jsonSerializerOptions) : IRedisMessageQueue<T>
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly string _queueKey = $"queue:{queueName}";
    private readonly string _deadLetterKey = $"queue:{queueName}:dead";
    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions;

    public async Task EnqueueAsync(T message, bool fireAndForget)
    {
        var wrapped = new BaseRedisQueueMessage<T> { Payload = message };
        var json = JsonSerializer.Serialize(wrapped, _jsonSerializerOptions);
        await _db.ListRightPushAsync(_queueKey, json, flags: fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
    }

    public async Task<BaseRedisQueueMessage<T>?> DequeueAsync(int retryDelay, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = RedisValue.Null;

            try
            {
                result = await _db.ListLeftPopAsync(_queueKey);
                if (result.HasValue)
                {
                    // Attempt to deserialize the message
                    return JsonSerializer.Deserialize<BaseRedisQueueMessage<T>>(
                        result.ToString(),
                        _jsonSerializerOptions);
                }
            }
            catch (RedisTimeoutException)
            {
                // Jump to the delay without trying to process an empty result.
                continue;
            }
            catch (JsonException)
            {
                // Push the original bad JSON string to the dead-letter key
                await _db.ListRightPushAsync(_deadLetterKey, result);
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            await Task.Delay(retryDelay, cancellationToken);
        }

        return null;
    }

    public async Task RequeueAsync(BaseRedisQueueMessage<T> message)
    {
        var json = JsonSerializer.Serialize(message, _jsonSerializerOptions);
        await _db.ListRightPushAsync(_queueKey, json);
    }

    public async Task MoveToDeadLetterAsync(BaseRedisQueueMessage<T> message)
    {
        var json = JsonSerializer.Serialize(message, _jsonSerializerOptions);
        await _db.ListRightPushAsync(_deadLetterKey, json);
    }

    public async Task HandleProcessingErrorAsync(
        IRedisMessageQueue<T> queue,
        BaseRedisQueueMessage<T> envelope,
        int retryLimit,
        int retryDelayMs,
        ILogger logger,
        Exception ex,
        CancellationToken stoppingToken)
    {
        envelope.RetryCount++;
        if (envelope.RetryCount >= retryLimit)
        {
            logger.LogWarning("Message failed after {RetryCount} retries. Moving to dead-letter queue.",
                envelope.RetryCount);
            await queue.MoveToDeadLetterAsync(envelope);
        }
        else
        {
            logger.LogWarning(ex, "Message failed. Retrying {RetryCount}/{MaxRetryCount}...",
                envelope.RetryCount, retryLimit);
            await Task.Delay(retryDelayMs, stoppingToken);
            await queue.RequeueAsync(envelope);
        }
    }
}


