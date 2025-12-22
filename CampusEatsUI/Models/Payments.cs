namespace CampusEatsUI.Models;

public record Payments
(
    Guid Id,
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