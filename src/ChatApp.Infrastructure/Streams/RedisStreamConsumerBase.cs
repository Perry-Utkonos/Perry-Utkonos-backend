namespace ChatApp.Infrastructure.Streams;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

/// <summary>
/// База для consumer-групп Redis Streams.
/// Гарантирует at-least-once: XACK только после успешной обработки.
/// </summary>
public abstract class RedisStreamConsumerBase : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger _logger;

    protected RedisStreamOptions Options { get; }

    /// <summary>Имя consumer-группы (например, "broadcast").</summary>
    protected abstract string GroupName { get; }

    /// <summary>Уникальное имя этого инстанса consumer'а.</summary>
    protected virtual string ConsumerName =>
        $"{Environment.MachineName}-{Guid.NewGuid():N}";

    protected RedisStreamConsumerBase(
        IConnectionMultiplexer redis,
        IOptions<RedisStreamOptions> opt,
        ILogger logger)
    {
        _redis = redis;
        _logger = logger;
        Options = opt.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _redis.GetDatabase();

        // Создать consumer group, если ещё нет. BUSYGROUP = уже создана, это не ошибка.
        try
        {
            await db.StreamCreateConsumerGroupAsync(
                Options.StreamName, GroupName, "0-0", createStream: true);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // group already exists
        }

        _logger.LogInformation(
            "{Consumer} started: stream={Stream} group={Group} consumer={ConsumerName}",
            GetType().Name, Options.StreamName, GroupName, ConsumerName);

        // Сначала дренируем pending (сообщения, доставленные до краха)
        await DrainPendingAsync(db, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(
                    Options.StreamName,
                    GroupName,
                    ConsumerName,
                    ">",                         // только новые сообщения
                    count: Options.ReadBatchSize,
                    flags: CommandFlags.None);

                if (entries.Length == 0)
                    continue;

                foreach (var entry in entries)
                {
                    await ProcessWithAckAsync(db, entry, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Consumer} read loop failed", GetType().Name);
                await Task.Delay(2000, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Обработать pending-сообщения, доставленные этому consumer'у до краха.
    /// Читаем с ID "0" — это всё, что уже было доставлено, но не подтверждено.
    /// </summary>
    private async Task DrainPendingAsync(IDatabase db, CancellationToken ct)
    {
        try
        {
            var pending = await db.StreamReadGroupAsync(
                Options.StreamName, GroupName, ConsumerName, "0",
                count: Options.ReadBatchSize);

            foreach (var entry in pending)
                await ProcessWithAckAsync(db, entry, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to drain pending messages");
        }
    }

    private async Task ProcessWithAckAsync(IDatabase db, StreamEntry entry, CancellationToken ct)
    {
        try
        {
            await HandleAsync(entry, ct);
            await db.StreamAcknowledgeAsync(Options.StreamName, GroupName, entry.Id);
        }
        catch (Exception ex)
        {
            // НЕ ACK — сообщение останется в pending и будет переобработано.
            _logger.LogError(ex,
                "{Consumer} failed to handle entry {Id}",
                GetType().Name, entry.Id);
        }
    }

    /// <summary>Обработать одно событие. Бросайте исключение — сообщение не будет подтверждено.</summary>
    protected abstract Task HandleAsync(StreamEntry entry, CancellationToken ct);

    protected static string? GetField(StreamEntry entry, string name)
    {
        foreach (var f in entry.Values)
            if (f.Name == name) return f.Value;
        return null;
    }
}
