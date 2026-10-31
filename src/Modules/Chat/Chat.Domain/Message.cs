namespace Chat.Domain;

public sealed class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public Guid ClientMessageId { get; set; }
    public long Sequence { get; set; }
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
