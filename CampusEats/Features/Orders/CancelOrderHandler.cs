using CampusEats.Exceptions;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;

public class CancelOrderHandler(CampusEatsContext context)
{
    public async Task<OrderResponse> Handle(CancelOrderRequest request)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id);

        if (order == null)
        {
            throw new OrderNotFoundException(request.Id);
        }

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOrderStatusException($"Cannot cancel order with status {order.Status}");
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

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
