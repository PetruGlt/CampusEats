namespace CampusEats.Features.Payment;

public record PaymentResponse(
    Guid PaymentId,
    string SessionId,
    string CheckoutUrl,
    string Status,
    long Amount,
    string Currency
);

