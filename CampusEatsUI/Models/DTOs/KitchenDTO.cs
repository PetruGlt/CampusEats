namespace CampusEatsUI.Models.DTOs;

public record UpdateOrderStatusRequestBody(string Status);
public record BulkUpdateOrderStatusRequest(List<Guid> OrderIds, string Status);
public record KitchenDashboardDto(int PendingOrdersCount, int PreparingOrdersCount, int CompletedOrdersCount);
public record PopularItemDto(string Name, int Count);