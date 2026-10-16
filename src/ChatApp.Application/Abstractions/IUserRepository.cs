namespace ChatApp.Application.Abstractions;

using ChatApp.Domain.Entities;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenIssuer
{
    Task<Dtos.AuthResponse> IssueAsync(User user, CancellationToken ct = default);
    Task<Dtos.AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);
}
