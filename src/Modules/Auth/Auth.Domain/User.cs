namespace Auth.Domain;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // В реальности тут будет хэш
    public string ZodiacSign { get; set; } = string.Empty;   // Овен, Телец и т.д.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
