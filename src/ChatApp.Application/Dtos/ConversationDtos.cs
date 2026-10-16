namespace ChatApp.Application.Dtos;

public record ConversationDto(
    Guid Id,
    UserDto Peer,
    string? LastMessageBody,
    DateTime? LastMessageAt,
    Guid? LastMessageSenderId,
    long LastSeq,
    int UnreadCount);

public record CreateConversationRequest(Guid PeerId);

public record MessagePageDto(
    IReadOnlyList<MessageDto> Items,
    long? NextCursor,
    bool HasMore);
