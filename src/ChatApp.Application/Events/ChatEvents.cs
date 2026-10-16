namespace ChatApp.Application.Events;

public sealed record MessageCreatedEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderId,
    long Seq,
    string Body) : IntegrationEvent
{
    public override string EventType => "message.created";
}

public sealed record MessageReadEvent(
    Guid ConversationId,
    Guid UserId,
    long LastReadSeq) : IntegrationEvent
{
    public override string EventType => "message.read";
}

public sealed record UserCreatedEvent(
    Guid UserId,
    string Username) : IntegrationEvent
{
    public override string EventType => "user.created";
}

public sealed record PaymentCreatedEvent(
    Guid PaymentId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string Purpose) : IntegrationEvent
{
    public override string EventType => "payment.created";
}

public sealed record PaymentCompletedEvent(
    Guid PaymentId,
    Guid UserId,
    string Purpose) : IntegrationEvent
{
    public override string EventType => "payment.completed";
}
