namespace ChatApp.Infrastructure.Services;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "chatapp";
    public string Audience { get; set; } = "chatapp-clients";
    public string SigningKey { get; set; } = default!;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 30;
}
