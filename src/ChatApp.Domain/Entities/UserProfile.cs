namespace ChatApp.Domain.Entities;

public class UserProfile
{
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
}
