namespace CampusEats.Features.Payment;

public record PaymentHistoryResponse(
    Guid PaymentId,
    Guid OrderId,
    long Amount,
    string Currency,
    string Status,
    string UserId,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? FailureReason,
    string? ReceiptUrl
);

