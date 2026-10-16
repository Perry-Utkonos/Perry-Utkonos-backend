namespace ChatApp.Application.Services;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using ChatApp.Domain.Exceptions;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messages;

    public MessageService(IMessageRepository messages) => _messages = messages;

    public async Task<MessageDto> SendAsync(
        Guid conversationId, Guid senderId, Guid clientMessageId, string body,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ValidationException("Message body is empty.");

        if (body.Length > 8000)
            throw new ValidationException("Message body is too long.");

        if (clientMessageId == Guid.Empty)
            throw new ValidationException("clientMessageId is required.");

        return await _messages.SendAsync(conversationId, senderId, clientMessageId, body, ct);
    }

    public Task MarkReadAsync(Guid conversationId, Guid userId, long lastReadSeq, CancellationToken ct = default)
        => _messages.MarkReadAsync(conversationId, userId, lastReadSeq, ct);

    public Task EnsureParticipantAsync(Guid conversationId, Guid userId, CancellationToken ct = default)
        => _messages.EnsureParticipantAsync(conversationId, userId, ct);
}
