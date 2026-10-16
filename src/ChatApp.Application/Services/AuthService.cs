namespace ChatApp.Application.Services;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;

/// <summary>
/// Сервис аутентификации. Работает через абстракции,
/// чтобы Application не зависел от EF Core напрямую.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenIssuer _tokens;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenIssuer tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
            throw new ValidationException("Username must be at least 3 characters.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("Password must be at least 8 characters.");

        if (await _users.ExistsByUsernameAsync(request.Username, ct))
            throw new ConflictException("Username is already taken.");

        if (await _users.ExistsByEmailAsync(request.Email, ct))
            throw new ConflictException("Email is already registered.");

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _hasher.Hash(request.Password),
            Profile = new UserProfile
            {
                Metadata = "{}"
            }
        };

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        return await _tokens.IssueAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.FindByUsernameOrEmailAsync(request.UsernameOrEmail, ct)
            ?? throw new ForbiddenException("Invalid credentials.");

        if (!user.IsActive)
            throw new ForbiddenException("Account is disabled.");

        if (!_hasher.Verify(request.Password, user.PasswordHash))
            throw new ForbiddenException("Invalid credentials.");

        return await _tokens.IssueAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
        => await _tokens.RefreshAsync(refreshToken, ct);
}
