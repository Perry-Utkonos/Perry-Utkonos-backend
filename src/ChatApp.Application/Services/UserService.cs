namespace ChatApp.Application.Services;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using ChatApp.Application.Events;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IEventBus _bus;

    public UserService(IUserRepository users, IPasswordHasher hasher, IEventBus bus)
    {
        _users = users;
        _hasher = hasher;
        _bus = bus;
    }

    public async Task<UserDto> CreateAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
            throw new ValidationException("Username must be at least 3 characters.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("Password must be at least 8 characters.");

        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.ExistsByUsernameAsync(username, ct))
            throw new ConflictException("Username is already taken.");
        if (await _users.ExistsByEmailAsync(email, ct))
            throw new ConflictException("Email is already registered.");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            Profile = new UserProfile { Metadata = "{}" }
        };

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        await _bus.PublishAsync(new UserCreatedEvent(user.Id, user.Username), ct);

        return ToDto(user);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id, ct);
        return user is null ? null : ToDto(user);
    }

    private static UserDto ToDto(User u) => new(
        u.Id, u.Username, u.Email,
        u.Profile?.Bio, u.Profile?.AvatarUrl, u.LastSeenAt);
}
