namespace ChatApp.Application.Abstractions;

using ChatApp.Application.Dtos;

public interface IPaymentService
{
    Task<PaymentDto> CreateAsync(Guid userId, CreatePaymentRequest req, CancellationToken ct = default);
    Task<PaymentDto?> GetByIdAsync(Guid userId, Guid paymentId, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDto>> ListForUserAsync(Guid userId, CancellationToken ct = default);
    Task<PaymentDto> CompleteAsync(Guid paymentId, CancellationToken ct = default);
}
