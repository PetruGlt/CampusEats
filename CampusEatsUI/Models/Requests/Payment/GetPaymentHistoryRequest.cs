namespace CampusEatsUI.Models.Requests.Payment;

public record GetPaymentHistoryRequest(string UserId, DateTime StartDate, DateTime EndDate, string Status);