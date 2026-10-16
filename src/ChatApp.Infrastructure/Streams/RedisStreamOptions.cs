namespace ChatApp.Infrastructure.Streams;

public class RedisStreamOptions
{
    public const string SectionName = "RedisStreams";

    public string StreamName { get; set; } = "chat:events";

    /// <summary>Consumer group для broadcast через SignalR.</summary>
    public string BroadcastGroup { get; set; } = "broadcast";

    /// <summary>Consumer group для push-уведомлений.</summary>
    public string NotificationGroup { get; set; } = "notifications";

    /// <summary>Consumer group для поискового индекса.</summary>
    public string SearchGroup { get; set; } = "search";

    /// <summary>Batch size для publisher'а.</summary>
    public int PublishBatchSize { get; set; } = 50;

    /// <summary>Интервал опроса outbox.</summary>
    public int PublishIntervalMs { get; set; } = 1000;

    /// <summary>Максимум событий за один XREADGROUP.</summary>
    public int ReadBatchSize { get; set; } = 20;

    /// <summary>Block timeout для XREADGROUP в миллисекундах.</summary>
    public int ReadBlockMs { get; set; } = 5000;

    /// <summary>Порог попыток, после которого событие считается мёртвым.</summary>
    public int MaxPublishAttempts { get; set; } = 10;
}