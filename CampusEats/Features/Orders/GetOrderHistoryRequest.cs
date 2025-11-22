namespace CampusEats.Features.Orders;

public record GetOrderHistoryRequest(DateTime? StartDate, DateTime? EndDate, string? UserId);
