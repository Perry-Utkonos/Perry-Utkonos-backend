namespace ChatApp.Api.Handlers;

using ChatApp.Application.Dtos;
using ChatApp.Application.Events;
using ChatApp.Api.Hubs;
using ChatApp.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

public class SignalRBroadcastHandler : IEventHandler<MessageCreatedEvent>
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;
    public SignalRBroadcastHandler(IHubContext<ChatHub, IChatClient> hub) => _hub = hub;

    public Task HandleAsync(MessageCreatedEvent evt, CancellationToken ct = default)
    {
        var dto = new MessageDto(
            evt.MessageId, evt.ConversationId, evt.SenderId, evt.Seq,
            evt.Body, MessageType.Text, null,
            evt.OccurredAt, null, null);

        return _hub.Clients
            .Group($"conv:{evt.ConversationId}")
            .ReceiveMessage(dto);
    }
}
