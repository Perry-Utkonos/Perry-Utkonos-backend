using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Perry.Api.BackgroundServices;

public class SessionCleanupWorker : BackgroundService
{
    private readonly UserSessionBuffer _sessionBuffer;
    private readonly ILogger<SessionCleanupWorker> _logger;

    // Время жизни сессии без активности (например, 1 час)
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(1);

    public SessionCleanupWorker(UserSessionBuffer sessionBuffer, ILogger<SessionCleanupWorker> logger)
    {
        _sessionBuffer = sessionBuffer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Cleanup Worker started.");

        // Запускаем проверку каждые 10 минут
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var removedCount = _sessionBuffer.CleanupExpired(_sessionTimeout);

            if (removedCount > 0)
            {
                _logger.LogInformation("Очищено {Count} протухших сессий. Осталось активных: {Active}",
                    removedCount, _sessionBuffer.Count);
            }
        }
    }
}
