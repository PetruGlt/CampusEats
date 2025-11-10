namespace CampusEats.Features.Orders;

public record CreateOrderRequest(
    string UserId,
    List<OrderItemDto> Items,
    string? Notes
);

public record OrderItemDto(
    Guid MenuItemId,
    int Quantity,
    string? SpecialInstructions
);
