namespace ChatApp.Application.Dtos;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string? DisplayName);

public record LoginRequest(
    string UsernameOrEmail,
    string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserDto User);

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? DisplayName,
    string? AvatarUrl,
    DateTime? LastSeenAt);
