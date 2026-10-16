using System.Text;
using ChatApp.Application.Abstractions;
using ChatApp.Application.Services;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Repositories;
using ChatApp.Infrastructure.Services;
using ChatApp.Infrastructure.Streams;
using ChatApp.Infrastructure.Streams.Consumers;
using StackExchange.Redis;
using ChatApp.Api.Hubs;
using ChatApp.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuration ----------
var config = builder.Configuration;

// ---------- DbContext ----------
builder.Services.AddDbContext<ChatDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Postgres"),
        npg => npg.MigrationsAssembly(typeof(ChatDbContext).Assembly.FullName)));

// ---------- Options ----------
builder.Services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));

// ---------- Auth ----------
var jwt = config.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt section is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // SignalR: JWT через query string (WebSocket не умеет Authorization header)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/chat"))
                {
                    ctx.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ---------- SignalR ----------
var signalR = builder.Services.AddSignalR();
var redisConn = config.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConn))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        _ => ConnectionMultiplexer.Connect(redisConn));
    signalR.AddStackExchangeRedis(redisConn, opts =>
    {
        opts.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("chatapp");
    });
}

builder.Services.Configure<RedisStreamOptions>(
    config.GetSection(RedisStreamOptions.SectionName));

builder.Services.AddSingleton<IChatBroadcaster, SignalRChatBroadcaster>();

// ---------- DI ----------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ITokenIssuer, JwtTokenIssuer>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();

// Publisher + Consumers как hosted services
builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddHostedService<ChatBroadcastConsumer>();
builder.Services.AddHostedService<NotificationConsumer>();

// ---------- MVC + Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.SetIsOriginAllowed(_ => true)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

var app = builder.Build();

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

// Автомиграция при старте (для dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    if (app.Environment.IsDevelopment())
        await db.Database.MigrateAsync();
}

app.Run();
