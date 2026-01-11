namespace CampusEatsUI.Models.Helpers;

public record PaymentResponse(
    Guid PaymentId,
    string SessionId,
    string CheckoutUrl,
    string Status,
    long Amount,
    string Currency
);