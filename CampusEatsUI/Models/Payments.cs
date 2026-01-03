namespace CampusEatsUI.Models;

public record Payments
(
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