public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string StripeSessionId { get; set; } = string.Empty;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public PaymentStatus Status { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? ReceiptUrl { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Cancelled
}