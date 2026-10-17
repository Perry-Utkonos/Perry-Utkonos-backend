using MediatR;

namespace Auth.Contracts;

// Запрос на вход
public record LoginCommand(string Username, string Password, string? ZodiacSign = null) : IRequest<LoginResult>;

// Результат: либо просим форму, либо даем токен
public record LoginResult(
    bool IsNewUser,
    bool Success,
    string? Token,
    string? FormData, // Например, "Заполните знак зодиака"
    string? ErrorMessage = null
);
