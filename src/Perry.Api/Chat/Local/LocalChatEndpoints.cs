using System.Net;
using Chat.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Perry.Api.Chat;

internal sealed record LocalLoginRequest(string Alias);

internal static class LocalChatEndpoints
{
    public static void MapLocalChat(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            throw new InvalidOperationException("ChatLocal разрешён только в Development");
        var group = app.MapGroup("/api/chat-local").WithTags("ChatLocal");
        group.AddEndpointFilter(new LoopbackOnlyFilter());
        group.MapGet("/users", () => Results.Ok(LocalChatUsers.All));
        group.MapPost("/token", (LocalLoginRequest request, LocalChatTokens tokens) =>
        {
            var user = LocalChatUsers.All.FirstOrDefault(x => string.Equals(x.Alias, request.Alias, StringComparison.OrdinalIgnoreCase));
            return user is null ? Results.BadRequest(new { message = "Выберите alice, bob или charlie" }) : Results.Ok(tokens.Issue(user));
        });
        group.MapGet("/health", async (ChatDbContext db, CancellationToken ct) =>
        {
            try
            {
                // Querying also verifies that the schema has been initialized.
                await db.Conversations.AsNoTracking().AnyAsync(ct);
                return Results.Ok(new { status = "ready" });
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                return Results.Problem(statusCode: 503, detail: "PostgreSQL или схема chat недоступны. Запустите compose.chat-local.yml.");
            }
        });

        var pages = Path.Combine(app.Environment.ContentRootPath, "Chat", "Local", "Pages");
        app.MapGet("/chat-local", () => Results.File(Path.Combine(pages, "index.html"), "text/html; charset=utf-8"))
            .AddEndpointFilter(new LoopbackOnlyFilter());
        app.MapGet("/chat-local/signalr.min.js", () => Results.File(Path.Combine(pages, "signalr.min.js"), "text/javascript; charset=utf-8"))
            .AddEndpointFilter(new LoopbackOnlyFilter());
    }

    private sealed class LoopbackOnlyFilter : IEndpointFilter
    {
        public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var http = context.HttpContext;
            if (http.Connection.RemoteIpAddress is not { } address || !IPAddress.IsLoopback(address))
                return ValueTask.FromResult<object?>(Results.StatusCode(403));
            http.Response.Headers.CacheControl = "no-store";
            return next(context);
        }
    }
}
