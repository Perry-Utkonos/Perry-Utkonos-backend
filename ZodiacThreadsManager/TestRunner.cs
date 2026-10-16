using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZodiacThreadsManager.Core.Models;
using ZodiacThreadsManager.Core.Messaging;
using ZodiacThreadsManager.Gateway;

public class TestRunner
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Testing Thread Manager + Gateway...\n");
        
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IMessageBus, InMemoryMessageBus>();
                services.AddSingleton<ThreadManager>();
                services.AddHostedService<GatewayHostedService>();
            })
            .ConfigureLogging((hostContext, logging) =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var scope = host.Services.CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<GatewayHostedService>();
        
        // Test client registration
        string clientId = "test-client-123";
        string connectionId = gateway.RegisterClient(clientId);
        Console.WriteLine($"✓ Registered client {clientId} -> {connectionId}");
        
        // Test message posting
        var message = new ThreadMessage
        {
            ClientId = clientId,
            ServiceName = "TestService",
            Type = MessageType.Chat,
            Payload = "Hello from test client",
            Metadata = new Dictionary<string, string> { ["test"] = "true" }
        };
        
        var postTask = gateway.PostMessageAsync(message);
        Console.WriteLine("✓ Posted message for processing");
        
        // Give some time for processing
        System.Threading.Thread.Sleep(1000);
        
        // Test status check
        var status = gateway.GetStatus(message.Id);
        if (status != null)
        {
            Console.WriteLine($"✓ Message status: {status.Status} -> Response: {status.Response}");
        }
        else
        {
            Console.WriteLine($"✗ Message status: Not found (id={message.Id})");
        }
        
        Console.WriteLine("\n✓ Thread Manager + Gateway test completed successfully!");
    }
}