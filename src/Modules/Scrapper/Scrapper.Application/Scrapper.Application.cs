public class UpdateDailyHoroscopesHandler : IRequestHandler<UpdateDailyHoroscopesCommand, List<HoroscopeResult>>
{
    private readonly IHoroscopeParser _parser;
    private readonly ILogger<UpdateDailyHoroscopesHandler> _logger;

    private static readonly string[] Signs = { "aries", "taurus", "gemini", "cancer", "leo", "virgo", "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces" };

    public UpdateDailyHoroscopesHandler(IHoroscopeParser parser, ILogger<UpdateDailyHoroscopesHandler> logger)
    {
        _parser = parser;
        _logger = logger;
    }

    public async Task<List<HoroscopeResult>> Handle(UpdateDailyHoroscopesCommand request, CancellationToken cancellationToken)
    {
        var results = new List<HoroscopeResult>();

        foreach (var sign in Signs)
        {
            try
            {
                _logger.LogInformation("Скраппим гороскоп для: {Sign}", sign);
                var text = await _parser.FetchHoroscopeAsync(sign, cancellationToken);
                results.Add(new HoroscopeResult(sign, text, true));

                // Задержка, чтобы сайт не забанил за DDOS
                await Task.Delay(2000, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при скраппинге {Sign}", sign);
                results.Add(new HoroscopeResult(sign, string.Empty, false, ex.Message));
            }
        }

        return results;
    }
}
