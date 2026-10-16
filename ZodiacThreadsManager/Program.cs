using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZodiacThreadsManager.Core.Messaging;
using ZodiacThreadsManager.Gateway;
using ZodiacThreadsManager;

public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IMessageBus, InMemoryMessageBus>();
                services.AddSingleton<ThreadManager>();
                services.AddHostedService<GatewayHostedService>();
            })
            .ConfigureLogging((hostContext, logging) =>
            {
                logging.AddConsole();
            })
            .Build()
            .Run();
    }
}