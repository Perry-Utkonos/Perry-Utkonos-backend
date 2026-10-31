using System.Collections.Concurrent;

namespace Perry.Api.BackgroundServices;

// Модель данных авторизованного пользователя
public class AuthorizedUser
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
}

// Сам буфер (Singleton)
public class UserSessionBuffer
{
    // Ключ - ID пользователя, Значение - его сессия
    private readonly ConcurrentDictionary<Guid, AuthorizedUser> _sessions = new();

    public void AddOrUpdate(AuthorizedUser user)
    {
        user.LastActive = DateTime.UtcNow;
        _sessions.AddOrUpdate(user.UserId, user, (key, oldValue) => user);
    }

    public AuthorizedUser? Get(Guid userId)
    {
        if (_sessions.TryGetValue(userId, out var user))
        {
            user.LastActive = DateTime.UtcNow; // Обновляем время активности при чтении
            return user;
        }
        return null;
    }

    public bool IsAuthorized(Guid userId) => _sessions.ContainsKey(userId);

    public void Remove(Guid userId) => _sessions.TryRemove(userId, out _);

    // Метод для очистки (вызывается из фонового воркера)
    public int CleanupExpired(TimeSpan expirationTime)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _sessions
            .Where(kvp => now - kvp.Value.LastActive > expirationTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _sessions.TryRemove(key, out _);
        }

        return expiredKeys.Count;
    }

    // Для диагностики
    public int Count => _sessions.Count;
}
