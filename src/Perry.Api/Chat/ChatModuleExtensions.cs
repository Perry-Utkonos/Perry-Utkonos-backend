using System.Security.Claims;
using Chat.Application;
using Chat.Contracts;
using Chat.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Perry.Api.Chat;

public static class ChatModuleExtensions
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connection = configuration.GetConnectionString("Chat")
            ?? throw new InvalidOperationException("Задайте ConnectionStrings:Chat для включения чата");
        services.AddChatInfrastructure(connection);
        var local = configuration.GetValue<bool>("Chat:Local:Enabled");
        if (local)
        {
            if (!environment.IsDevelopment())
                throw new InvalidOperationException("Локальная авторизация разрешена только в Development");
            var tokens = new LocalChatTokens();
            services.AddSingleton(tokens);
            services.AddScoped<IChatPolicy, LocalChatPolicy>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokens.ValidationParameters;
                options.Events = CreateJwtEvents();
            });
        }
        else
        {
            var authority = configuration["Chat:Authentication:Authority"];
            var audience = configuration["Chat:Authentication:Audience"];
            if (!Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri) || authorityUri.Scheme != "https" || string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("Задайте HTTPS Chat:Authentication:Authority и Chat:Authentication:Audience");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.Events = CreateJwtEvents();
            });
        }
        services.AddAuthorization();
        services.AddSignalR();
        services.AddHostedService<ChatDispatcher>();
        return services;
    }

    private static JwtBearerEvents CreateJwtEvents() => new()
    {
        OnMessageReceived = context =>
        {
            if (context.HttpContext.Request.Path.StartsWithSegments("/hubs/chat") &&
                context.Request.Query.TryGetValue("access_token", out var token))
                context.Token = token.ToString();
            return Task.CompletedTask;
        }
    };

    public static IEndpointRouteBuilder MapChatModule(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat").RequireAuthorization().WithTags("Chat");
        group.AddEndpointFilter(async (context, next) =>
        {
            try { return await next(context); }
            catch (ChatException ex) { return Results.Problem(statusCode: ex.Status, detail: ex.Message); }
        });
        group.MapPost("/conversations", async (CreateRequest r, ClaimsPrincipal user, IChatService service, CancellationToken ct) =>
            Results.Ok(await service.Create(ChatIdentity.UserId(user), r.PeerId, ct)));
        group.MapGet("/conversations", async (ClaimsPrincipal user, IChatService service, CancellationToken ct) =>
            Results.Ok(await service.List(ChatIdentity.UserId(user), ct)));
        group.MapGet("/conversations/{id:guid}/messages", async (Guid id, long? before, long? after, int? limit,
            ClaimsPrincipal user, IChatService service, CancellationToken ct) =>
            Results.Ok(await service.History(id, ChatIdentity.UserId(user), before, after, limit ?? 50, ct)));
        group.MapPost("/conversations/{id:guid}/messages", async (Guid id, SendRequest r, ClaimsPrincipal user, IChatService service, CancellationToken ct) =>
            Results.Ok(await service.Send(id, ChatIdentity.UserId(user), r, ct)));
        group.MapPut("/conversations/{id:guid}/read", async (Guid id, ReadRequest r, ClaimsPrincipal user, IChatService service, CancellationToken ct) =>
        {
            await service.MarkRead(id, ChatIdentity.UserId(user), r.Sequence, ct);
            return Results.NoContent();
        });
        group.MapGet("/conversations/{id:guid}/reads", async (Guid id, ClaimsPrincipal user, IChatService service, CancellationToken ct) =>
            Results.Ok(await service.GetReads(id, ChatIdentity.UserId(user), ct)));
        app.MapHub<ChatHub>("/hubs/chat", options => options.CloseOnAuthenticationExpiration = true).RequireAuthorization();
        return app;
    }
}
