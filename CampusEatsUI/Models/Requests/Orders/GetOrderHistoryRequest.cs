namespace CampusEatsUI.Models.Requests.Orders;

public record GetOrderHistoryRequest(DateTime StartDate, DateTime EndDate, Guid UserId);