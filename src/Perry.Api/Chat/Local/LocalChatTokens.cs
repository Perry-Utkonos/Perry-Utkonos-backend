using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Perry.Api.Chat;

internal sealed class LocalChatTokens
{
    private const string Issuer = "perry-chat-local";
    private const string Audience = "perry-chat-local-api";
    // A new key for every API process. Restarting invalidates previously issued local tokens.
    private readonly SymmetricSecurityKey key = new(RandomNumberGenerator.GetBytes(32));

    public TokenValidationParameters ValidationParameters => new()
    {
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ClockSkew = TimeSpan.FromSeconds(10)
    };

    public object Issue(LocalChatUser user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddHours(2);
        var token = new JwtSecurityToken(Issuer, Audience,
            [new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
             new Claim(ClaimTypes.Name, user.Alias),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())],
            notBefore: now, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new { user, accessToken = new JwtSecurityTokenHandler().WriteToken(token), expiresAt = expires };
    }
}
