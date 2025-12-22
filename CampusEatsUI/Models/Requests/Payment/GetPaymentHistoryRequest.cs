namespace CampusEatsUI.Models.Requests.Payment;

public record GetPaymentHistoryRequest(Guid UserId, DateTime StartDate, DateTime EndDate, string Status);