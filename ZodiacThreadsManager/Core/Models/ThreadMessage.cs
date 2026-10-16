namespace ZodiacThreadsManager.Core.Models;

public enum MessageType
{
    Chat,
    Tarot,
    WebScrape,
    HexParse,
    Recommendation,
    System
}

public enum MessageStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class ThreadMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClientId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Pending;
    public string Payload { get; set; } = string.Empty;
    public string? Response { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class ClientConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public bool IsAuthenticated { get; set; }
}
