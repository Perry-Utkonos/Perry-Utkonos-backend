namespace Chat.Domain;

public sealed class OutboxEvent
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Payload { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
