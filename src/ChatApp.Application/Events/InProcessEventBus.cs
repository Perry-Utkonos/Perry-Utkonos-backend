namespace ChatApp.Application.Events;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// In-process реализация IEventBus. Обработчики вызываются синхронно в том же процессе.
/// </summary>
public class InProcessEventBus : IEventBus
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<InProcessEventBus> _logger;

    public InProcessEventBus(IServiceProvider sp, ILogger<InProcessEventBus> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T evt, CancellationToken ct = default)
        where T : IIntegrationEvent
    {
        _logger.LogDebug("Publishing {Type} {EventId}", evt.EventType, evt.EventId);

        var handlers = _sp.GetServices<IEventHandler<T>>().ToList();
        if (handlers.Count == 0) return;

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(evt, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Handler {Handler} failed for {Type} {EventId}",
                    handler.GetType().Name, evt.EventType, evt.EventId);
            }
        }
    }
}
