using CampusEats.Exceptions;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;

public class CreateOrderHandler(CampusEatsContext context)
{
    public async Task<OrderResponse> Handle(CreateOrderRequest request)
    {
        // Validate and fetch menu items
        var menuItemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var menuItems = await context.MenuItems
            .Where(m => menuItemIds.Contains(m.Id))
            .ToListAsync();

        if (menuItems.Count != menuItemIds.Count)
        {
            var foundIds = menuItems.Select(m => m.Id).ToList();
            var missingIds = menuItemIds.Except(foundIds).ToList();
            throw new MenuItemsNotFoundException(missingIds);
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Notes = request.Notes,
            Items = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var itemDto in request.Items)
        {
            var menuItem = menuItems.First(m => m.Id == itemDto.MenuItemId);
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                MenuItemId = menuItem.Id,
                MenuItemName = menuItem.Name,
                Price = menuItem.Price,
                Quantity = itemDto.Quantity,
                SpecialInstructions = itemDto.SpecialInstructions
            };

            order.Items.Add(orderItem);
            totalAmount += menuItem.Price * itemDto.Quantity;
        }

        order.TotalAmount = totalAmount;

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return new OrderResponse(
            order.Id,
            order.UserId,
            order.Items.Select(i => new OrderItemResponse(
                i.Id,
                i.MenuItemId,
                i.MenuItemName,
                i.Price,
                i.Quantity,
                i.SpecialInstructions
            )).ToList(),
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            order.Notes
        );
    }
}

public record OrderResponse(
    Guid Id,
    Guid UserId,
    List<OrderItemResponse> Items,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Notes
);

public record OrderItemResponse(
    Guid Id,
    Guid MenuItemId,
    string MenuItemName,
    decimal Price,
    int Quantity,
    string? SpecialInstructions
);
