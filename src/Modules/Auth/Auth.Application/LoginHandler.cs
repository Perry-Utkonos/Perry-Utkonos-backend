using MediatR;
using Auth.Contracts;
using Auth.Domain;
using Auth.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly AuthDbContext _db;
    // Позже сюда добавим JwtGenerator

    public LoginHandler(AuthDbContext db)
    {
        _db = db;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Ищем юзера в БД
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        // 2. Если юзера нет (НОВЫЙ)
        if (user == null)
        {
            // Если знак зодиака не передан, просим форму
            if (string.IsNullOrEmpty(request.ZodiacSign))
            {
                return new LoginResult(IsNewUser: true, Success: false, Token: null, FormData: "Пожалуйста, укажите ваш знак зодиака для регистрации.");
            }

            // Создаем нового (в реальности тут надо хэшировать пароль!)
            user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = request.Password, // TODO: Хэшировать!
                ZodiacSign = request.ZodiacSign
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
        }

        // 3. Юзер есть (или только что создан) -> Генерируем токен
        // (Код генерации JWT напишем чуть позже, пока заглушка)
        var fakeToken = $"mock-jwt-token-for-{user.Id}";

        // 4. Возвращаем успех
        return new LoginResult(IsNewUser: false, Success: true, Token: fakeToken, FormData: null);
    }
}
