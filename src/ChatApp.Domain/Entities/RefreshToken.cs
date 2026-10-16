namespace ChatApp.Domain.Entities;

/// <summary>
/// Refresh-токен для обновления access-токена без повторного логина.
/// Хранится хеш, а не сам токен.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    // Навигация
    public User User { get; set; } = default!;
}
