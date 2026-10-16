using Microsoft.Extensions.Logging;
using ZodiacThreadsManager.Core.Messaging;
using ZodiacThreadsManager.Core.Models;

namespace ZodiacThreadsManager.Core.Services;

public abstract class ThreadServiceBase : IThreadService
{
    public abstract string Name { get; }
    protected readonly ILogger<ThreadServiceBase> Logger;
    protected readonly IMessageBus Bus;

    protected ThreadServiceBase(ILogger<ThreadServiceBase> logger, IMessageBus bus)
    {
        Logger = logger;
        Bus = bus;
    }

    public abstract Task ProcessAsync(ThreadMessage message, CancellationToken ct = default);

    protected async Task PublishResponseAsync(ThreadMessage original, string response, CancellationToken ct)
    {
        var outMsg = new ThreadMessage
        {
            Id = Guid.NewGuid(),
            ClientId = original.ClientId,
            ServiceName = original.ServiceName,
            Type = original.Type,
            Status = MessageStatus.Completed,
            Payload = original.Payload,
            Response = response,
            Metadata = original.Metadata,
        };
        await Bus.PublishAsync(outMsg, ct);
    }
}