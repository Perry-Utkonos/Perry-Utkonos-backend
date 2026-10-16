namespace ChatApp.Infrastructure.Repositories;

using ChatApp.Application.Abstractions;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly ChatDbContext _db;
    public RecommendationRepository(ChatDbContext db) => _db = db;

    public async Task<IReadOnlyList<Recommendation>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Recommendations
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Score)
            .Take(50)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<Recommendation> items, CancellationToken ct = default)
        => await _db.Recommendations.AddRangeAsync(items, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
