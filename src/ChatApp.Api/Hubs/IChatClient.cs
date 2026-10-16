namespace ChatApp.Api.Hubs;

using ChatApp.Application.Dtos;

/// <summary>
/// Строго типизированный контракт клиента SignalR.
/// Избавляет от магических строк в SendAsync("ReceiveMessage", ...).
/// </summary>
public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);
    Task ReadReceipt(Guid conversationId, Guid userId, long lastReadSeq);
    Task UserTyping(Guid conversationId, Guid userId);
}
