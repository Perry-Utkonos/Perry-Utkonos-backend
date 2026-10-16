namespace ChatApp.Application.Dtos;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string? DisplayName);

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? DisplayName,
    string? AvatarUrl,
    DateTime? LastSeenAt);
