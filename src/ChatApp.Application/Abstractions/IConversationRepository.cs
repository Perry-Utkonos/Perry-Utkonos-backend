namespace ChatApp.Application.Abstractions;

using ChatApp.Application.Dtos;

public interface IConversationRepository
{
    Task<IReadOnlyList<ConversationDto>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetConversationIdsAsync(Guid userId, CancellationToken ct = default);
    Task<ConversationDto> CreateOrGetAsync(Guid currentUserId, Guid userA, Guid userB, CancellationToken ct = default);
    Task<MessagePageDto> GetMessagesAsync(Guid conversationId, long? beforeSeq, int limit, CancellationToken ct = default);
    Task EnsureParticipantAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
}
