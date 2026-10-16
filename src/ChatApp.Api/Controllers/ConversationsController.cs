namespace ChatApp.Api.Controllers;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversations;
    private readonly ICurrentUserProvider _currentUser;

    public ConversationsController(IConversationService conversations, ICurrentUserProvider currentUser)
    {
        _conversations = conversations;
        _currentUser = currentUser;
    }

    [HttpGet]
    public Task<IReadOnlyList<ConversationDto>> List()
        => _conversations.GetUserConversationsAsync(_currentUser.UserId);

    [HttpPost]
    public Task<ConversationDto> CreateOrGet([FromBody] CreateConversationRequest req)
        => _conversations.CreateOrGetAsync(_currentUser.UserId, req.PeerId);

    [HttpGet("{id:guid}/messages")]
    public Task<MessagePageDto> Messages(Guid id, [FromQuery] long? before, [FromQuery] int limit = 50)
        => _conversations.GetMessagesAsync(_currentUser.UserId, id, before, limit);
}
