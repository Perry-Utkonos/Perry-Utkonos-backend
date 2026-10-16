namespace ChatApp.Api.Controllers;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    public AuthController(IUserService users) => _users = users;

    /// <summary>
    /// Создать пользователя. Токен не выдаётся — для последующих запросов
    /// используйте заголовок X-User-Id.
    /// </summary>
    [HttpPost("register")]
    public Task<UserDto> Register([FromBody] RegisterRequest req)
        => _users.CreateAsync(req);
}
