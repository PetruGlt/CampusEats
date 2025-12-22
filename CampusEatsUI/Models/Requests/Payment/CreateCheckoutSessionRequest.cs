namespace CampusEatsUI.Models.Requests.Payment;

public record CreateCheckoutSessionRequest(Guid Id, Guid UserId, string SuccessUrl, string CancelUrl);