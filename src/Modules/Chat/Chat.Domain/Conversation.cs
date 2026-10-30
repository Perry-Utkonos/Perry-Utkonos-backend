namespace Chat.Domain;

public sealed class Conversation
{
    public Guid Id { get; set; }
    public Guid UserA { get; set; }
    public Guid UserB { get; set; }
    public long Sequence { get; set; }
    public DateTime CreatedAt { get; set; }
}
