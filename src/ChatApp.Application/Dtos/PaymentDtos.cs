namespace ChatApp.Application.Dtos;

public record CreatePaymentRequest(
    decimal Amount,
    string Currency,
    string Purpose);

public record PaymentDto(
    Guid Id,
    Guid UserId,
    decimal Amount,
    string Currency,
    string Purpose,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt);
