
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scrapper.Contracts;


//using Jev.Infrastructure;

namespace Perry.Api.BackgroundServices;

public class ScheduleWorker : BackgroundService
{
    private readonly TaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduleWorker> _logger;

    public ScheduleWorker(TaskQueue taskQueue, IServiceScopeFactory scopeFactory, ILogger<ScheduleWorker> logger)
    {
        _taskQueue = taskQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Schedule (Heart) starting...");

        // 1. Запускаем фоновую задачу для ежедневного сбора гороскопов
        var dailyTask = RunDailyHoroscopeJobAsync(stoppingToken);

        // 2. Параллельно слушаем очередь (для ручных запросов от Gateway)
        var queueTask = ListenToQueueAsync(stoppingToken);

        await Task.WhenAll(dailyTask, queueTask);

        _logger.LogInformation("Schedule (Heart) stopped.");
    }

    private async Task RunDailyHoroscopeJobAsync(CancellationToken stoppingToken)
    {
        // Ждем 24 часа между запусками
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            _logger.LogInformation("=== Запуск ежедневного сбора гороскопов ===");

            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var dbContext = scope.ServiceProvider.GetRequiredService<JevDbContext>();

            try
            {
                // 1. Скраппим
                var horoscopes = await mediator.Send(new UpdateDailyHoroscopesCommand(), stoppingToken);

                // 2. Сохраняем в БД
                foreach (var h in horoscopes.Where(x => x.Success))
                {
                    var entity = new Jev.Domain.DailyHoroscope
                    {
                        ZodiacSign = h.ZodiacSign,
                        Text = h.HoroscopeText,
                        Date = DateTime.UtcNow.Date
                    };
                    dbContext.DailyHoroscopes.Add(entity);
                }

                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("=== Гороскопы успешно обновлены и сохранены в БД ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в ежедневном джобе гороскопов");
            }

        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ListenToQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Ждем задачу от пользователя (например, Guid юзера или запрос на обновление ленты)
                var requestData = await _taskQueue.DequeueAsync(stoppingToken);
                _logger.LogInformation("Получен ручной запрос из очереди: {Data}", requestData);

                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var dbContext = scope.ServiceProvider.GetRequiredService<JevDbContext>();

                // Логика: если юзер просит обновить ленту, мы БЕРЕМ ГОРОСКОП ИЗ БД,
                // а не идем в Scrapper!

                var todayHoroscope = dbContext.DailyHoroscopes
                    .FirstOrDefault(x => x.ZodiacSign == "aries" && x.Date == DateTime.UtcNow.Date);

                if (todayHoroscope != null)
                {
                    _logger.LogInformation("Гороскоп на сегодня найден в БД. Передаем в Recommendation...");
                    // TODO: await mediator.Send(new GenerateRecommendationCommand(requestData, todayHoroscope.Text));
                }
                else
                {
                    _logger.LogWarning("Гороскопа на сегодня нет в БД! (Джоб еще не отработал)");
                    // Здесь можно сделать fallback: принудительно вызвать Scrapper
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса из очереди");
            }
        }
    }
}
