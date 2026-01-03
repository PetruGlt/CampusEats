namespace CampusEats.Features.Payment;

public record CreatePaymentRequest(
    Guid OrderId,
    Guid UserId,
    string SuccessUrl,
    string CancelUrl
);

