namespace ChatApp.Application.Abstractions;

public interface ICurrentUserProvider
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}
