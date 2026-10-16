namespace ChatApp.Api.Hubs;

using ChatApp.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// Тонкий транспортный слой. Вся логика — в сервисах.
/// Группа conv:{id} содержит обоих участников диалога.
/// </summary>
[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IMessageService _messages;
    private readonly IConversationService _conversations;

    public ChatHub(IMessageService messages, IConversationService conversations)
    {
        _messages = messages;
        _conversations = conversations;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var convIds = await _conversations.GetConversationIdsAsync(userId);

        foreach (var id in convIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(id));

        await base.OnConnectedAsync();
    }

    public async Task SendMessage(Guid conversationId, Guid clientMessageId, string body)
    {
        var userId = GetUserId();
        await _messages.SendAsync(conversationId, userId, clientMessageId, body);
    }

    public async Task MarkRead(Guid conversationId, long lastReadSeq)
    {
        var userId = GetUserId();
        await _messages.MarkReadAsync(conversationId, userId, lastReadSeq);

        await Clients.OthersInGroup(GroupName(conversationId))
                     .ReadReceipt(conversationId, userId, lastReadSeq);
    }

    public async Task Typing(Guid conversationId)
    {
        var userId = GetUserId();
        await _messages.EnsureParticipantAsync(conversationId, userId);

        await Clients.OthersInGroup(GroupName(conversationId))
                     .UserTyping(conversationId, userId);
    }

    private Guid GetUserId()
    {
        var raw = Context.UserIdentifier
            ?? throw new HubException("Unauthenticated.");
        return Guid.Parse(raw);
    }

    private static string GroupName(Guid conversationId) => $"conv:{conversationId}";
}
