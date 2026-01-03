namespace CampusEatsUI.Models.Requests.Payment;

public record CreateCheckoutSessionRequest(
    Guid OrderId, 
    Guid UserId, 
    string SuccessUrl, 
    string CancelUrl);