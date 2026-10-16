namespace ChatApp.Application.Events;

public interface IEventBus
{
    Task PublishAsync<T>(T evt, CancellationToken ct = default)
        where T : IIntegrationEvent;
}
