namespace ChatApp.Infrastructure.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenIssuer : ITokenIssuer
{
    private readonly JwtOptions _opt;
    private readonly ChatDbContext _db;

    public JwtTokenIssuer(IOptions<JwtOptions> opt, ChatDbContext db)
    {
        _opt = opt.Value;
        _db = db;
    }

    public async Task<AuthResponse> IssueAsync(User user, CancellationToken ct = default)
    {
        var accessToken = CreateAccessToken(user, out var expiresAt);
        var refreshToken = await CreateAndStoreRefreshTokenAsync(user, ct);

        return new AuthResponse(
            accessToken,
            refreshToken,
            expiresAt,
            ToDto(user));
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = HashToken(refreshToken);

        var stored = await _db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new ForbiddenException("Invalid refresh token.");

        if (!stored.IsActive)
            throw new ForbiddenException("Refresh token is expired or revoked.");

        stored.RevokedAt = DateTime.UtcNow;
        var response = await IssueAsync(stored.User, ct);
        await _db.SaveChangesAsync(ct);

        return response;
    }

    private string CreateAccessToken(User user, out DateTime expiresAt)
    {
        expiresAt = DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateAndStoreRefreshTokenAsync(User user, CancellationToken ct)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashToken(raw);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays)
        });
        await _db.SaveChangesAsync(ct);

        return raw;
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static UserDto ToDto(User u) => new(
        u.Id,
        u.Username,
        u.Email,
        u.Profile?.Bio,
        u.Profile?.AvatarUrl,
        u.LastSeenAt);
}
