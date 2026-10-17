using MediatR;

namespace Scrapper.Contracts;

// Команда: "Достань гороскоп для этого знака"
public record GetHoroscopeCommand(string ZodiacSign) : IRequest<HoroscopeResult>;

// Результат: "Вот текст гороскопа"
public record HoroscopeResult(string ZodiacSign, string HoroscopeText, bool Success, string? ErrorMessage = null);
