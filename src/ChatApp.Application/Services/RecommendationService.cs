namespace ChatApp.Application.Services;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;

public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationRepository _repo;
    public RecommendationService(IRecommendationRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<RecommendationDto>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await _repo.GetForUserAsync(userId, ct);
        return list.Select(r => new RecommendationDto(
            r.Id, r.TargetUserId, r.Score, r.Reason, r.CreatedAt)).ToList();
    }

    public Task RebuildForUserAsync(Guid userId, CancellationToken ct = default)
    {
        // TODO: интеграция с ML-сервисом
        return Task.CompletedTask;
    }
}
