namespace CampusEats.Features.Orders;

public record GetOrderHistoryRequest(DateTime? StartDate, DateTime? EndDate, Guid UserId);
