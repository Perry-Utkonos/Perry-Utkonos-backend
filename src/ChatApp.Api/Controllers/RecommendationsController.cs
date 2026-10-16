namespace ChatApp.Api.Controllers;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recs;
    private readonly ICurrentUserProvider _currentUser;

    public RecommendationsController(IRecommendationService recs, ICurrentUserProvider currentUser)
    {
        _recs = recs;
        _currentUser = currentUser;
    }

    [HttpGet]
    public Task<IReadOnlyList<RecommendationDto>> List()
        => _recs.GetForUserAsync(_currentUser.UserId);

    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild()
    {
        await _recs.RebuildForUserAsync(_currentUser.UserId);
        return Accepted();
    }
}
