using Chat.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Chat.Infrastructure;

public static class ChatInfrastructureExtensions
{
    public static IServiceCollection AddChatInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.AddDbContext<ChatDbContext>(o => o.UseNpgsql(connectionString));
        services.AddScoped<IChatService, ChatService>();
        services.TryAddScoped<IChatPolicy, UnconfiguredChatPolicy>();
        return services;
    }
}

internal sealed class UnconfiguredChatPolicy : IChatPolicy
{
    public Task<bool> CanCreate(Guid actor, Guid peer, CancellationToken ct) =>
        throw new ChatException(503, "Монолит ещё не подключил правила доступа IChatPolicy");
    public Task<bool> CanSend(Guid actor, Guid peer, CancellationToken ct) =>
        throw new ChatException(503, "Монолит ещё не подключил правила доступа IChatPolicy");
}
