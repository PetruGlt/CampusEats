namespace CampusEats.Features.Payment;

public record CreatePaymentRequest(
    Guid OrderId,
    string UserId,
    string SuccessUrl,
    string CancelUrl
);

