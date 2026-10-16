namespace ChatApp.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public long LastSeq { get; set; }
    public Guid? LastMessageId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessageBody { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User UserA { get; set; } = default!;
    public User UserB { get; set; } = default!;
    public Message? LastMessage { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<MessageRead> Reads { get; set; } = new List<MessageRead>();

    public static (Guid a, Guid b) OrderPair(Guid x, Guid y)
        => x.CompareTo(y) < 0 ? (x, y) : (y, x);
}
