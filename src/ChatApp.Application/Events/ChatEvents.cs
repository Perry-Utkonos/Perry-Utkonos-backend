namespace ChatApp.Application.Events;

/// <summary>
/// Базовый тип события. Все события чата сериализуются в JSON
/// и публикуются в Redis Stream chat:events.
/// </summary>
public abstract record ChatEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

public sealed record MessageCreatedEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderId,
    long Seq,
    string Body) : ChatEvent
{
    public override string EventType => "message.created";
}

public sealed record MessageReadEvent(
    Guid ConversationId,
    Guid UserId,
    long LastReadSeq) : ChatEvent
{
    public override string EventType => "message.read";
}
