namespace ChatApp.Api.Hubs;

using ChatApp.Application.Dtos;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);
    Task ReadReceipt(Guid conversationId, Guid userId, long lastReadSeq);
    Task UserTyping(Guid conversationId, Guid userId);
}
