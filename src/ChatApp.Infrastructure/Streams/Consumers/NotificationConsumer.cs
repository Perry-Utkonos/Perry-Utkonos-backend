namespace ChatApp.Infrastructure.Streams.Consumers;

using System.Text.Json;
using ChatApp.Application.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

public class NotificationConsumer : RedisStreamConsumerBase
{
    protected override string GroupName => Options.NotificationGroup;

    public NotificationConsumer(
        IConnectionMultiplexer redis,
        IOptions<RedisStreamOptions> opt,
        ILogger<NotificationConsumer> logger)
        : base(redis, opt, logger) { }

    protected override Task HandleAsync(StreamEntry entry, CancellationToken ct)
    {
        var type = GetField(entry, "type");
        if (type != "message.created") return Task.CompletedTask;

        var payload = GetField(entry, "payload");
        if (string.IsNullOrEmpty(payload)) return Task.CompletedTask;

        var evt = JsonSerializer.Deserialize<MessageCreatedEvent>(payload);
        if (evt is null) return Task.CompletedTask;

        // TODO: проверить last_seen_at получателя; если оффлайн — отправить FCM/APNs.
        // Пока это заглушка, которая просто логирует.
        Console.WriteLine($"[Notification] new message in conv {evt.ConversationId}");

        return Task.CompletedTask;
    }
}
