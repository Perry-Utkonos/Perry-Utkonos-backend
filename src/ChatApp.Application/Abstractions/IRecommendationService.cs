namespace ChatApp.Application.Abstractions;

using ChatApp.Application.Dtos;

public interface IRecommendationService
{
    Task<IReadOnlyList<RecommendationDto>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task RebuildForUserAsync(Guid userId, CancellationToken ct = default);
}
