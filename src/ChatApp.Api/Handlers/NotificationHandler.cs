namespace ChatApp.Api.Handlers;

using ChatApp.Application.Events;

public class NotificationHandler : IEventHandler<MessageCreatedEvent>
{
    private readonly ILogger<NotificationHandler> _logger;
    public NotificationHandler(ILogger<NotificationHandler> logger) => _logger = logger;

    public Task HandleAsync(MessageCreatedEvent evt, CancellationToken ct = default)
    {
        // TODO: если получатель оффлайн — отправить FCM/APNs
        _logger.LogInformation(
            "[NOTIFY] new message {MsgId} in conv {ConvId} from {Sender}",
            evt.MessageId, evt.ConversationId, evt.SenderId);
        return Task.CompletedTask;
    }
}
