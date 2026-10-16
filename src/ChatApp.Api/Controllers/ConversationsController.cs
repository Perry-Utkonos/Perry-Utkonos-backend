namespace ChatApp.Api.Controllers;

using System.Security.Claims;
using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversations;

    public ConversationsController(IConversationService conversations)
        => _conversations = conversations;

    [HttpGet]
    public Task<IReadOnlyList<ConversationDto>> List()
        => _conversations.GetUserConversationsAsync(CurrentUserId());

    [HttpPost]
    public Task<ConversationDto> CreateOrGet([FromBody] CreateConversationRequest req)
        => _conversations.CreateOrGetAsync(CurrentUserId(), req.PeerId);

    [HttpGet("{id:guid}/messages")]
    public Task<MessagePageDto> Messages(
        Guid id,
        [FromQuery] long? before,
        [FromQuery] int limit = 50)
        => _conversations.GetMessagesAsync(CurrentUserId(), id, before, limit);

    private Guid CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(raw);
    }
}
