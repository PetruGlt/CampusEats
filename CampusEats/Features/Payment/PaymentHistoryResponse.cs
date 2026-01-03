namespace CampusEats.Features.Payment;

public record PaymentHistoryResponse(
    Guid PaymentId,
    Guid OrderId,
    long Amount,
    string Currency,
    string Status,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? FailureReason,
    string? ReceiptUrl
);

