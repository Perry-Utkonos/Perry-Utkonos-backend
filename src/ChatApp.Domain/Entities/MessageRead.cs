namespace ChatApp.Domain.Entities;

/// <summary>
/// Позиция прочтения пользователя в диалоге.
/// Составной PK: (conversation_id, user_id). Ровно одна строка на пару.
/// </summary>
public class MessageRead
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>До какого seq пользователь дочитал диалог (включительно).</summary>
    public long LastReadSeq { get; set; }

    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    // Навигация
    public Conversation Conversation { get; set; } = default!;
    public User User { get; set; } = default!;
}
