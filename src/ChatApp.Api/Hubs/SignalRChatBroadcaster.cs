namespace ChatApp.Api.Hubs;

using ChatApp.Application.Events;
using ChatApp.Infrastructure.Streams.Consumers;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// Реализует IChatBroadcaster через IHubContext<ChatHub, IChatClient>.
/// Именно она делает broadcast в SignalR, а не сам ChatHub.
/// </summary>
public class SignalRChatBroadcaster : IChatBroadcaster
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public SignalRChatBroadcaster(IHubContext<ChatHub, IChatClient> hub) => _hub = hub;

    public Task BroadcastMessageAsync(MessageCreatedEvent evt, CancellationToken ct = default)
    {
        var dto = new Application.Dtos.MessageDto(
            evt.MessageId,
            evt.ConversationId,
            evt.SenderId,
            evt.Seq,
            evt.Body,
            Domain.Enums.MessageType.Text,
            null,
            evt.OccurredAt,
            null,
            null);

        return _hub.Clients
            .Group($"conv:{evt.ConversationId}")
            .ReceiveMessage(dto);
    }
}