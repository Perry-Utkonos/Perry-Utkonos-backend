namespace ChatApp.Application.Abstractions;

using ChatApp.Application.Dtos;

public interface IConversationService
{
    Task<IReadOnlyList<ConversationDto>> GetUserConversationsAsync(Guid userId, CancellationToken ct = default);
    Task<ConversationDto> CreateOrGetAsync(Guid userId, Guid peerId, CancellationToken ct = default);
    Task<MessagePageDto> GetMessagesAsync(
        Guid userId, Guid conversationId, long? beforeSeq, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetConversationIdsAsync(Guid userId, CancellationToken ct = default);
}
