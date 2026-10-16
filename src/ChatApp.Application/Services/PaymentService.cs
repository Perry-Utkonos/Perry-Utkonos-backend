namespace ChatApp.Application.Services;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using ChatApp.Application.Events;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repo;
    private readonly IEventBus _bus;

    public PaymentService(IPaymentRepository repo, IEventBus bus)
    {
        _repo = repo;
        _bus = bus;
    }

    public async Task<PaymentDto> CreateAsync(Guid userId, CreatePaymentRequest req, CancellationToken ct = default)
    {
        if (req.Amount <= 0)
            throw new ValidationException("Amount must be positive.");

        var payment = new Payment
        {
            UserId = userId,
            Amount = req.Amount,
            Currency = req.Currency,
            Purpose = req.Purpose,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(payment, ct);
        await _repo.SaveChangesAsync(ct);

        await _bus.PublishAsync(new PaymentCreatedEvent(
            payment.Id, userId, payment.Amount, payment.Currency, payment.Purpose), ct);

        return ToDto(payment);
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid userId, Guid paymentId, CancellationToken ct = default)
    {
        var p = await _repo.FindByIdAsync(paymentId, ct);
        return p is null || p.UserId != userId ? null : ToDto(p);
    }

    public async Task<IReadOnlyList<PaymentDto>> ListForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await _repo.GetForUserAsync(userId, ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<PaymentDto> CompleteAsync(Guid paymentId, CancellationToken ct = default)
    {
        var p = await _repo.FindByIdAsync(paymentId, ct)
            ?? throw new NotFoundException("Payment not found.");

        if (p.Status == PaymentStatus.Completed)
            return ToDto(p);

        p.Status = PaymentStatus.Completed;
        p.CompletedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);

        await _bus.PublishAsync(new PaymentCompletedEvent(p.Id, p.UserId, p.Purpose), ct);
        return ToDto(p);
    }

    private static PaymentDto ToDto(Payment p) => new(
        p.Id, p.UserId, p.Amount, p.Currency, p.Purpose,
        p.Status.ToString().ToLowerInvariant(),
        p.CreatedAt, p.CompletedAt);
}
