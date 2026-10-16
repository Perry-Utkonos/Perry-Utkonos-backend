namespace ChatApp.Api.Hubs;

using ChatApp.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR Hub. Идентификатор пользователя берётся из ICurrentUserProvider
/// (заголовок X-User-Id или ?userId=).
/// Broadcast уходит через event bus → SignalRBroadcastHandler.
/// </summary>
public class ChatHub : Hub<IChatClient>
{
    private readonly IMessageService _messages;
    private readonly IConversationService _conversations;
    private readonly ICurrentUserProvider _currentUser;

    public ChatHub(IMessageService messages, IConversationService conversations, ICurrentUserProvider currentUser)
    {
        _messages = messages;
        _conversations = conversations;
        _currentUser = currentUser;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUser.UserId;
        var convIds = await _conversations.GetConversationIdsAsync(userId);
        foreach (var id in convIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(id));
        await base.OnConnectedAsync();
    }

    public Task SendMessage(Guid conversationId, Guid clientMessageId, string body)
        => _messages.SendAsync(conversationId, _currentUser.UserId, clientMessageId, body);

    public async Task MarkRead(Guid conversationId, long lastReadSeq)
    {
        var userId = _currentUser.UserId;
        await _messages.MarkReadAsync(conversationId, userId, lastReadSeq);
        await Clients.OthersInGroup(GroupName(conversationId))
                     .ReadReceipt(conversationId, userId, lastReadSeq);
    }

    public async Task Typing(Guid conversationId)
    {
        var userId = _currentUser.UserId;
        await _messages.EnsureParticipantAsync(conversationId, userId);
        await Clients.OthersInGroup(GroupName(conversationId))
                     .UserTyping(conversationId, userId);
    }

    private static string GroupName(Guid conversationId) => $"conv:{conversationId}";
}
