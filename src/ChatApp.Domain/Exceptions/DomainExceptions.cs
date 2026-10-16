namespace ChatApp.Domain.Exceptions;

/// <summary>Базовое доменное исключение. Маппится в HTTP 400.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>Пользователь не найден. HTTP 404.</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Доступ запрещён. HTTP 403.</summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>Конфликт (например, username занят). HTTP 409.</summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Ошибка валидации. HTTP 400.</summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
}
