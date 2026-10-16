namespace ChatApp.Domain.Entities;

/// <summary>
/// Аккаунт пользователя. Корневая сущность identity.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }

    /// <summary>BCrypt-хеш пароля. Никогда не покидает сервер.</summary>
    public string PasswordHash { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    /// <summary>Обновляется presence-событиями SignalR.</summary>
    public DateTime? LastSeenAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Навигация
    public UserProfile? Profile { get; set; }
    public ICollection<Conversation> ConversationsAsUserA { get; set; } = new List<Conversation>();
    public ICollection<Conversation> ConversationsAsUserB { get; set; } = new List<Conversation>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
