using Chat.Application;

namespace Perry.Api.Chat;

internal sealed class LocalChatPolicy(IHostEnvironment environment) : IChatPolicy
{
    public Task<bool> CanCreate(Guid actor, Guid peer, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(environment.IsDevelopment() && actor != peer
            && LocalChatUsers.Contains(actor) && LocalChatUsers.Contains(peer));
    }

    public Task<bool> CanSend(Guid actor, Guid peer, CancellationToken ct) => CanCreate(actor, peer, ct);
}
