namespace Chat.Application;

// Implement in the host: check matching, blocks, account status and other business rules.
public interface IChatPolicy
{
    Task<bool> CanCreate(Guid actor, Guid peer, CancellationToken ct);
    Task<bool> CanSend(Guid actor, Guid peer, CancellationToken ct);
}
