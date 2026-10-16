namespace ChatApp.Domain.Entities;

public class MessageRead
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public long LastReadSeq { get; set; }
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    public Conversation Conversation { get; set; } = default!;
    public User User { get; set; } = default!;
}
