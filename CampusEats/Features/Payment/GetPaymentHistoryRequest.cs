namespace CampusEats.Features.Payment;

public record GetPaymentHistoryRequest(
    string? UserId,
    DateTime? StartDate,
    DateTime? EndDate,
    PaymentStatus? Status
);

