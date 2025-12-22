using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Models.Requests.Orders;

public record CreateOrderRequest(Guid UserId, List<OrderItem> Items, string? Notes);