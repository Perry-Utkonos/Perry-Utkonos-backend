namespace ChatApp.Domain.Entities;

/// <summary>
/// Публичный профиль пользователя. Связь 1:1 с <see cref="User"/>.
/// PK = FK = user_id — классический shared primary key.
/// </summary>
public class UserProfile
{
    /// <summary>PK и FK одновременно.</summary>
    public Guid UserId { get; set; }

    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Gender { get; set; }

    /// <summary>Произвольные настройки (JSONB).</summary>
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигация
    public User User { get; set; } = default!;
}
