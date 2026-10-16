namespace ChatApp.Infrastructure.Repositories;

using ChatApp.Application.Abstractions;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class PaymentRepository : IPaymentRepository
{
    private readonly ChatDbContext _db;
    public PaymentRepository(ChatDbContext db) => _db = db;

    public Task<Payment?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Payments.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Payment>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Payments
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
        => await _db.Payments.AddAsync(payment, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
