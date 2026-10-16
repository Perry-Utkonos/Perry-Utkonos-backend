namespace ChatApp.Domain.Entities;

/// <summary>
/// Устаревшая сущность. Оставлена для совместимости со старой миграцией.
/// В текущей (in-process EventBus) версии не используется.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public string? Error { get; set; }
    public int Attempts { get; set; }
}
