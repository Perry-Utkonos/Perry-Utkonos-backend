using ZodiacThreadsManager.Core.Models;

namespace ZodiacThreadsManager.Core.Messaging;

public interface IMessageBus
{
    Task PublishAsync(ThreadMessage message, CancellationToken ct = default);
    Task SubscribeAsync(string serviceName, Func<ThreadMessage, CancellationToken, Task> handler, CancellationToken ct = default);
    Task<bool> TryAckAsync(Guid messageId, string serviceName, CancellationToken ct = default);
}
