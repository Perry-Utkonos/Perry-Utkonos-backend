using Microsoft.Extensions.Logging;
using ZodiacThreadsManager.Core.Models;
using ZodiacThreadsManager.Core.Messaging;

namespace ZodiacThreadsManager.Gateway;

public class GatewayHostedService : BackgroundService
{
    private readonly IMessageBus _bus;
    private readonly IServiceProvider _sp;
    private readonly ILogger<GatewayHostedService> _logger;
    private readonly Dictionary<string, string> _clientConnections = new();
    private readonly object _lock = new();

    public GatewayHostedService(IMessageBus bus, IServiceProvider sp, ILogger<GatewayHostedService> logger)
    {
        _bus = bus;
        _sp = sp;
        _logger = logger;
    }

    public string RegisterClient(string clientId)
    {
        var connId = Guid.NewGuid().ToString("N");
        lock (_lock) { _clientConnections[connId] = clientId; }
        return connId;
    }

    public async Task<bool> PostMessageAsync(ThreadMessage msg, CancellationToken ct)
    {
        // Store message for processing
        await _bus.PublishAsync(msg, ct);
        return true;
    }

    public ThreadMessage? GetStatus(Guid id) => _bus is Core.Messaging.InMemoryMessageBus b 
        ? b.GetPending(id) : null;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Register thread manager as a service handler
        var tm = _sp.GetService<ThreadManager>();
        if (tm != null)
        {
            tm.RegisterService("ThreadManager");
        }

        // Simple in-process async queue for POST/GET messages
        _logger.LogInformation("Gateway started, accepting async GET/POST messages");

        // Background task to process incoming messages from clients
        _ = Task.Run(async () =>
        {
            // This simulates async message receiving from external clients
            // In real implementation, this would connect to WebSocket, SignalR, etc.
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(500, stoppingToken);
            }
        }, stoppingToken);
    }
}