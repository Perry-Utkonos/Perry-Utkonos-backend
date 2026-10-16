namespace ChatApp.Application.Abstractions;

using ChatApp.Domain.Entities;

public interface IRecommendationRepository
{
    Task<IReadOnlyList<Recommendation>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Recommendation> items, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
