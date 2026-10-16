namespace ChatApp.Application.Dtos;

using ChatApp.Domain.Enums;

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    long Seq,
    string? Body,
    MessageType Type,
    Guid? ReplyToId,
    DateTime CreatedAt,
    DateTime? EditedAt,
    DateTime? DeletedAt);
