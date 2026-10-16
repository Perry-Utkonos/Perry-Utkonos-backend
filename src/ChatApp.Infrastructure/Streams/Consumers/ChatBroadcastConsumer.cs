namespace ChatApp.Infrastructure.Streams.Consumers;

using System.Text.Json;
using ChatApp.Application.Dtos;
using ChatApp.Application.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

public interface IChatBroadcaster
{
    Task BroadcastMessageAsync(MessageCreatedEvent evt, CancellationToken ct = default);
}

public class ChatBroadcastConsumer : RedisStreamConsumerBase
{
    private readonly IChatBroadcaster _broadcaster;

    protected override string GroupName => Options.BroadcastGroup;

    public ChatBroadcastConsumer(
        IConnectionMultiplexer redis,
        IOptions<RedisStreamOptions> opt,
        IChatBroadcaster broadcaster,
        ILogger<ChatBroadcastConsumer> logger)
        : base(redis, opt, logger)
    {
        _broadcaster = broadcaster;
    }

    protected override async Task HandleAsync(StreamEntry entry, CancellationToken ct)
    {
        var type = GetField(entry, "type");
        if (type != "message.created") return;

        var payload = GetField(entry, "payload");
        if (string.IsNullOrEmpty(payload)) return;

        var evt = JsonSerializer.Deserialize<MessageCreatedEvent>(payload);
        if (evt is null) return;

        await _broadcaster.BroadcastMessageAsync(evt, ct);
    }
}
