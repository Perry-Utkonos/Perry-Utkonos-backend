using AngleSharp;
using AngleSharp.Html.Dom;

namespace Scrapper.Infrastructure;

public interface IHoroscopeParser
{
    Task<string> FetchHoroscopeAsync(string zodiacSign, CancellationToken ct = default);
}

public class HoroscopeParser : IHoroscopeParser
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config; // Например, URL сайта берем из appsettings

    public HoroscopeParser(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> FetchHoroscopeAsync(string zodiacSign, CancellationToken ct = default)
    {
        // В реальности тут будет URL конкретного сайта, например:
        // var url = $"https://horo.mail.ru/prediction/{zodiacSign}/today/";

        // Для примера возьмем заглушку (пока не знаем точный сайт)
        var url = $"https://example.com/horoscope/{zodiacSign}";

        // 1. Скачиваем HTML
        var html = await _httpClient.GetStringAsync(url, ct);

        // 2. Парсим через AngleSharp
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html), ct);

        // 3. Вытаскиваем текст (селекторы зависят от реального сайта!)
        // Предположим, гороскоп лежит в <div class="horoscope-text">
        var horoscopeElement = document.QuerySelector(".horoscope-text");

        if (horoscopeElement == null)
            throw new Exception("Не удалось найти текст гороскопа на странице");

        return horoscopeElement.TextContent.Trim();
    }
}
