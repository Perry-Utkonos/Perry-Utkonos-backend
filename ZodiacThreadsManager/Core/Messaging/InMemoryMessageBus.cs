using ZodiacThreadsManager.Core.Models;

namespace ZodiacThreadsManager.Core.Messaging;

public class InMemoryMessageBus : IMessageBus
{
    private readonly Dictionary<string, List<Func<ThreadMessage, CancellationToken, Task>>> _handlers = new();
    private readonly Dictionary<Guid, ThreadMessage> _pending = new();
    private readonly object _lock = new();

    public Task PublishAsync(ThreadMessage message, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _pending[message.Id] = message;
        }

        List<Func<ThreadMessage, CancellationToken, Task>> targets;
        lock (_lock)
        {
            targets = _handlers.TryGetValue(message.ServiceName, out var h) ? new List<Func<ThreadMessage, CancellationToken, Task>>(h) : new();
        }

        foreach (var handler in targets)
        {
            _ = handler(message, ct);
        }

        return Task.CompletedTask;
    }

    public Task SubscribeAsync(string serviceName, Func<ThreadMessage, CancellationToken, Task> handler, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_handlers.TryGetValue(serviceName, out var list))
            {
                list = new();
                _handlers[serviceName] = list;
            }
            list.Add(handler);
        }
        return Task.CompletedTask;
    }

    public Task<bool> TryAckAsync(Guid messageId, string serviceName, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_pending.Remove(messageId));
        }
    }

    public ThreadMessage? GetPending(Guid id)
    {
        lock (_lock)
        {
            return _pending.TryGetValue(id, out var m) ? m : null;
        }
    }
}
