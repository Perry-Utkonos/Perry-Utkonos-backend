namespace ChatApp.Application.Abstractions;

using ChatApp.Domain.Entities;

public interface IPaymentRepository
{
    Task<Payment?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
