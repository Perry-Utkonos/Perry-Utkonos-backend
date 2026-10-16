namespace ChatApp.Application.Abstractions;

using ChatApp.Application.Dtos;

public interface IUserService
{
    Task<UserDto> CreateAsync(RegisterRequest request, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
