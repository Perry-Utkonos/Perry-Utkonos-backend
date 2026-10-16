namespace ChatApp.Infrastructure.Streams;

using System.Text.Json;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

/// <summary>
/// Фоновый сервис: читает outbox_messages, публикует в Redis Stream.
/// Работает по принципу at-least-once. Идемпотентность — на стороне consumer'ов.
/// </summary>
public class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisStreamOptions _opt;
    private readonly ILogger<OutboxPublisher> _logger;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IConnectionMultiplexer redis,
        IOptions<RedisStreamOptions> opt,
        ILogger<OutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _redis = redis;
        _opt = opt.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxPublisher started, stream={Stream}", _opt.StreamName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var published = await PublishBatchAsync(stoppingToken);
                if (published == 0)
                    await Task.Delay(_opt.PublishIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisher iteration failed");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task<int> PublishBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var redisDb = _redis.GetDatabase();

        var batch = await db.OutboxMessages
            .Where(m => m.PublishedAt == null && m.Attempts < _opt.MaxPublishAttempts)
            .OrderBy(m => m.CreatedAt)
            .Take(_opt.PublishBatchSize)
            .ToListAsync(ct);

        if (batch.Count == 0)
            return 0;

        var published = 0;
        foreach (var msg in batch)
        {
            try
            {
                // XADD stream * type <eventType> payload <json>
                await redisDb.StreamAddAsync(_opt.StreamName, new NameValueEntry[]
                {
                    new("type", msg.Type),
                    new("payload", msg.Payload),
                    new("eventId", msg.Id.ToString())
                });

                msg.PublishedAt = DateTime.UtcNow;
                msg.Error = null;
                published++;
            }
            catch (Exception ex)
            {
                msg.Attempts++;
                msg.Error = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                _logger.LogWarning(ex,
                    "Failed to publish outbox message {Id} (attempt {Attempts})",
                    msg.Id, msg.Attempts);
            }
        }

        await db.SaveChangesAsync(ct);
        return published;
    }
}
