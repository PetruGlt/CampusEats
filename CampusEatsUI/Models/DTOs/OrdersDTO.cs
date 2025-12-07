namespace CampusEatsUI.Models.DTOs;

public record CreateOrderRequest(string UserId, List<OrderItemDto> Items, string? Notes);
public record OrderItemDto(Guid MenuItemId, int Quantity, string? SpecialInstructions);
public record OrderDto(Guid Id, string UserId, string Status, decimal TotalAmount, DateTime CreatedAt, List<OrderItemResponseDto> Items);
public record OrderItemResponseDto(string MenuItemName, int Quantity, decimal Price);
public record OrderHistoryDto(Guid Id, DateTime CreatedAt, decimal TotalAmount, string Status);
public record OrderStatisticsDTO(int TotalOrders, decimal TotalRevenue, decimal AverageOrderValue, Dictionary<string, int> OrdersByStatus, int TodayOrders, decimal TodayRevenue );
public record SearchOrdersRequest(string? Query, string? Status);