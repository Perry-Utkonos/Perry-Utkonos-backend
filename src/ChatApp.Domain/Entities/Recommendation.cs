namespace ChatApp.Domain.Entities;

public class Recommendation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid TargetUserId { get; set; }
    public double Score { get; set; }
    public string Reason { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
