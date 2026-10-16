namespace ChatApp.Application.Abstractions;

using ChatApp.Application.Dtos;

public interface IMessageService
{
    Task<MessageDto> SendAsync(Guid conversationId, Guid senderId, Guid clientMessageId, string body, CancellationToken ct = default);
    Task MarkReadAsync(Guid conversationId, Guid userId, long lastReadSeq, CancellationToken ct = default);
    Task EnsureParticipantAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
}
