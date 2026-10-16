using Microsoft.Extensions.Logging;
using ZodiacThreadsManager.Core.Models;
using ZodiacThreadsManager.Core.Messaging;

namespace ZodiacThreadsManager;

public class ThreadManager
{
    private readonly IMessageBus _bus;
    private readonly ILogger<ThreadManager> _logger;

    public ThreadManager(IMessageBus bus, ILogger<ThreadManager> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public void RegisterService(string serviceName)
    {
        _bus.SubscribeAsync(serviceName, async (msg, ct) =>
        {
            await Task.Run(async () => { ProcessMessage(msg, ct); await Task.CompletedTask; }, ct);
        });
    }

    private void ProcessMessage(ThreadMessage msg, CancellationToken ct)
    {
        msg.Status = MessageStatus.Processing;
        msg.ProcessedAt = DateTime.UtcNow;

        try
        {
            // Simplified - in real implementation, this would call the actual service
            _logger.LogInformation("Processing message {Id} for {Service}", msg.Id, msg.ServiceName);
            
            // For demo, just echo back with processed status
            var response = $"Processed: {msg.Payload}";
            var outMsg = new ThreadMessage
            {
                Id = Guid.NewGuid(),
                ClientId = msg.ClientId,
                ServiceName = "ThreadManager",
                Type = msg.Type,
                Status = MessageStatus.Completed,
                Payload = msg.Payload,
                Response = response,
                Metadata = msg.Metadata,
            };
            
            _bus.PublishAsync(outMsg, ct);
        }
        catch (Exception ex)
        {
            msg.Status = MessageStatus.Failed;
            msg.Error = ex.Message;
            _logger.LogError(ex, "Failed processing {Id}", msg.Id);
        }
    }
}