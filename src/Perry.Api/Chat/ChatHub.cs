using System.Security.Claims;
using Chat.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Perry.Api.Chat;

public static class ChatIdentity
{
    public static Guid UserId(ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"), out var id) && id != Guid.Empty
            ? id : throw new ChatException(401, "Нужна авторизация с UUID пользователя");
}

[Authorize]
public sealed class ChatHub : Hub
{
    // Events go only to the authenticated user's connections; no client-supplied group subscription.
    public override async Task OnConnectedAsync()
    {
        var id = ChatIdentity.UserId(Context.User!);
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-user:{id}");
        await base.OnConnectedAsync();
    }
}
