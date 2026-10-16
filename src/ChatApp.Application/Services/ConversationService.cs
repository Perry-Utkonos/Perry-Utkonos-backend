namespace ChatApp.Application.Services;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using ChatApp.Domain.Exceptions;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversations;
    private readonly IUserRepository _users;

    public ConversationService(IConversationRepository conversations, IUserRepository users)
    {
        _conversations = conversations;
        _users = users;
    }

    public Task<IReadOnlyList<ConversationDto>> GetUserConversationsAsync(Guid userId, CancellationToken ct = default)
        => _conversations.GetForUserAsync(userId, ct);

    public Task<IReadOnlyList<Guid>> GetConversationIdsAsync(Guid userId, CancellationToken ct = default)
        => _conversations.GetConversationIdsAsync(userId, ct);

    public async Task<ConversationDto> CreateOrGetAsync(Guid userId, Guid peerId, CancellationToken ct = default)
    {
        if (userId == peerId)
            throw new ValidationException("Cannot create a conversation with yourself.");

        var peer = await _users.FindByIdAsync(peerId, ct)
            ?? throw new NotFoundException("Peer user not found.");

        var (a, b) = Domain.Entities.Conversation.OrderPair(userId, peerId);
        return await _conversations.CreateOrGetAsync(userId, a, b, ct);
    }

    public async Task<MessagePageDto> GetMessagesAsync(
        Guid userId, Guid conversationId, long? beforeSeq, int limit, CancellationToken ct = default)
    {
        if (limit is < 1 or > 100) limit = 50;
        await _conversations.EnsureParticipantAsync(conversationId, userId, ct);
        return await _conversations.GetMessagesAsync(conversationId, beforeSeq, limit, ct);
    }
}
