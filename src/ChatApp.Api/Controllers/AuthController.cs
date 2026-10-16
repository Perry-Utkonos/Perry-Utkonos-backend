namespace ChatApp.Api.Controllers;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public Task<AuthResponse> Register([FromBody] RegisterRequest req)
        => _auth.RegisterAsync(req);

    [HttpPost("login")]
    public Task<AuthResponse> Login([FromBody] LoginRequest req)
        => _auth.LoginAsync(req);

    [HttpPost("refresh")]
    public Task<AuthResponse> Refresh([FromBody] RefreshRequest req)
        => _auth.RefreshAsync(req.RefreshToken);
}
