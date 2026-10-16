namespace ChatApp.Domain.Entities;

/// <summary>
/// Метаданные вложения. Сам файл лежит в S3/MinIO, в БД — только ключ.
/// </summary>
public class Attachment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }

    public string FileName { get; set; } = default!;
    public string MimeType { get; set; } = default!;
    public long SizeBytes { get; set; }

    /// <summary>Ключ объекта в S3/MinIO (например: "chat/2026/10/uuid.png").</summary>
    public string StorageKey { get; set; } = default!;

    public int? Width { get; set; }
    public int? Height { get; set; }

    /// <summary>SHA-256 для дедупликации и проверки целостности.</summary>
    public string? Checksum { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигация
    public Message Message { get; set; } = default!;
}
