using ChatApp.Application.Abstractions;
using ChatApp.Application.Events;
using ChatApp.Application.Services;
using ChatApp.Api.Handlers;
using ChatApp.Api.Hubs;
using ChatApp.Api.Middleware;
using ChatApp.Api.Services;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Repositories;
using ChatApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ---------- DbContext ----------
builder.Services.AddDbContext<ChatDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Postgres"),
        npg => npg.MigrationsAssembly(typeof(ChatDbContext).Assembly.FullName)));

// ---------- Event Bus ----------
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();
builder.Services.AddScoped<IEventHandler<MessageCreatedEvent>, NotificationHandler>();
builder.Services.AddScoped<IEventHandler<MessageCreatedEvent>, SearchIndexerHandler>();
builder.Services.AddScoped<IEventHandler<MessageCreatedEvent>, SignalRBroadcastHandler>();

// ---------- Redis (опционально, для SignalR backplane) ----------
var redisConn = config.GetConnectionString("Redis");
var hasRedis = !string.IsNullOrWhiteSpace(redisConn);
if (hasRedis)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        _ => ConnectionMultiplexer.Connect(redisConn!));
}

// ---------- SignalR ----------
var signalR = builder.Services.AddSignalR(o =>
    o.EnableDetailedErrors = builder.Environment.IsDevelopment());

if (hasRedis)
{
    signalR.AddStackExchangeRedis(redisConn!, opts =>
    {
        opts.Configuration.ChannelPrefix = RedisChannel.Literal("chatapp");
    });
}

// ---------- Identity ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();

// ---------- Repositories ----------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();

// ---------- Application services ----------
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try { await db.Database.MigrateAsync(); }
    catch (Exception ex)
    {
        logger.LogError(ex, "Не удалось применить миграции. Проверьте connection string.");
    }
}

app.Run();
