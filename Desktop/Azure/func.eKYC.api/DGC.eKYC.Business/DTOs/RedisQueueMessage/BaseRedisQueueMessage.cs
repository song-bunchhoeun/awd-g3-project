namespace DGC.eKYC.Business.DTOs.RedisQueueMessage;

public class BaseRedisQueueMessage<T>
{
    /// <summary>
    /// The number of times this message has been retried.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// The actual payload (your domain message).
    /// </summary>
    public T Payload { get; set; } = default!; // Non-null assertion is appropriate for deserialization
}
