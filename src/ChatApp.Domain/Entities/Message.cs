namespace ChatApp.Domain.Entities;

using ChatApp.Domain.Enums;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public long Seq { get; set; }
    public string? Body { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public Guid? ReplyToId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Conversation Conversation { get; set; } = default!;
    public User Sender { get; set; } = default!;
    public Message? ReplyTo { get; set; }
    public ICollection<Message> Replies { get; set; } = new List<Message>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public bool IsDeleted => DeletedAt is not null;
}
