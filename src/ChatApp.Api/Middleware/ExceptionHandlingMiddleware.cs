namespace ChatApp.Api.Middleware;

using System.Net;
using System.Text.Json;
using ChatApp.Domain.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (Exception ex) { await HandleAsync(ctx, ex); }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var (status, message) = ex switch
        {
            NotFoundException e => (HttpStatusCode.NotFound, e.Message),
            ForbiddenException e => (HttpStatusCode.Forbidden, e.Message),
            ConflictException e => (HttpStatusCode.Conflict, e.Message),
            ValidationException e => (HttpStatusCode.BadRequest, e.Message),
            DomainException e => (HttpStatusCode.BadRequest, e.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized."),
            _ => (HttpStatusCode.InternalServerError, "Internal server error.")
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception");

        if (ctx.Response.HasStarted) return;

        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = message,
            status = (int)status
        }));
    }
}
