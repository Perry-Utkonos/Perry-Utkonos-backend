namespace ChatApp.Domain.Entities;

/// <summary>
/// Transactional Outbox. Пишется в ТОЙ ЖЕ транзакции, что и бизнес-данные.
/// Гарантирует: если сообщение в messages — событие рано или поздно уйдёт в брокер.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Тип события, например "message.created".</summary>
    public string Type { get; set; } = default!;

    /// <summary>Сериализованный payload (JSON).</summary>
    public string Payload { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Когда опубликовано. NULL = ещё не отправлено.</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>Последняя ошибка публикации (если была).</summary>
    public string? Error { get; set; }

    public int Attempts { get; set; }
}
