namespace ChatApp.Domain.Entities;

using ChatApp.Domain.Enums;

/// <summary>
/// Сообщение в диалоге. Append-only + soft-delete через <see cref="DeletedAt"/>.
/// </summary>
public class Message
{
    /// <summary>PK. Генерируется КЛИЕНТОМ (идемпотентность при повторной отправке).</summary>
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }

    /// <summary>Порядковый номер внутри диалога. Уникален в паре (conversation_id, seq).</summary>
    public long Seq { get; set; }

    public string? Body { get; set; }

    public MessageType Type { get; set; } = MessageType.Text;

    /// <summary>Self-reference: ответ на другое сообщение.</summary>
    public Guid? ReplyToId { get; set; }

    /// <summary>Доп. поля (реакции, форматирование) в виде JSONB.</summary>
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Навигация
    public Conversation Conversation { get; set; } = default!;
    public User Sender { get; set; } = default!;
    public Message? ReplyTo { get; set; }
    public ICollection<Message> Replies { get; set; } = new List<Message>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public bool IsDeleted => DeletedAt is not null;
}
