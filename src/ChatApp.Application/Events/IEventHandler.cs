namespace ChatApp.Application.Events;

public interface IEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent evt, CancellationToken ct = default);
}
