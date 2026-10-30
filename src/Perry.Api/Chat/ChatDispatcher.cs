using System.Text.Json;
using Chat.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Perry.Api.Chat;

internal sealed class ChatDispatcher(IServiceScopeFactory scopes, IHubContext<ChatHub> hub, ILogger<ChatDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
                await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);
                var events = await db.Outbox.FromSqlRaw("""
                    SELECT * FROM chat."Outbox" WHERE "ProcessedAt" IS NULL
                    ORDER BY "CreatedAt" LIMIT 20 FOR UPDATE SKIP LOCKED
                    """).ToListAsync(stoppingToken);
                foreach (var e in events)
                {
                    var c = await db.Conversations.AsNoTracking().SingleAsync(x => x.Id == e.ConversationId, stoppingToken);
                    await hub.Clients.Groups($"chat-user:{c.UserA}", $"chat-user:{c.UserB}")
                        .SendAsync("MessageCreated", new { eventId = e.Id, message = JsonSerializer.Deserialize<JsonElement>(e.Payload) }, stoppingToken);
                    e.ProcessedAt = DateTime.UtcNow;
                }
                await db.SaveChangesAsync(stoppingToken);
                await tx.CommitAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Chat outbox dispatch failed"); }
            try { await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
