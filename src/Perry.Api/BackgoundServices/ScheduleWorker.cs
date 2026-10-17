using MediatR;

namespace Perry.Api.BackgroundServices;

public class ScheduleWorker : BackgroundService
{
    private readonly TaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduleWorker> _logger;

    public ScheduleWorker(
        TaskQueue taskQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduleWorker> logger)
    {
        _taskQueue = taskQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Schedule (Heart) starting...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string url;
                try
                {
                    url = await _taskQueue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                try
                {
                    // Пока команд нет — это будет падать, но это ок
                    // Вернёмся сюда после создания модуля Scrapper
                    await mediator.Send(new { }, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing URL: {Url}", url);
                }
            }
        }
        finally
        {
            _logger.LogInformation("Schedule (Heart) stopped.");
        }
    }
}
