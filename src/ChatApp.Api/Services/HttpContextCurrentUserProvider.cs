namespace ChatApp.Api.Services;

using ChatApp.Application.Abstractions;
using ChatApp.Domain.Exceptions;

/// <summary>
/// Временная замена авторизации.
/// REST: заголовок X-User-Id
/// SignalR: query-параметр ?userId=
/// </summary>
public class HttpContextCurrentUserProvider : ICurrentUserProvider
{
    public const string HeaderName = "X-User-Id";
    public const string QueryName = "userId";

    private readonly IHttpContextAccessor _http;
    private Guid? _cached;
    private bool _resolved;

    public HttpContextCurrentUserProvider(IHttpContextAccessor http) => _http = http;

    public bool IsAuthenticated => TryResolve(out _);

    public Guid UserId
    {
        get
        {
            if (!TryResolve(out var id))
                throw new ForbiddenException(
                    $"User identity is missing. Provide '{HeaderName}' header " +
                    $"or '{QueryName}' query parameter.");
            return id;
        }
    }

    private bool TryResolve(out Guid id)
    {
        if (_resolved) { id = _cached ?? Guid.Empty; return _cached.HasValue; }
        _resolved = true;
        id = Guid.Empty;

        var ctx = _http.HttpContext;
        if (ctx is null) return false;

        if (ctx.Request.Headers.TryGetValue(HeaderName, out var h) &&
            Guid.TryParse(h.ToString(), out id))
        {
            _cached = id; return true;
        }

        if (ctx.Request.Query.TryGetValue(QueryName, out var q) &&
            Guid.TryParse(q.ToString(), out id))
        {
            _cached = id; return true;
        }

        return false;
    }
}
