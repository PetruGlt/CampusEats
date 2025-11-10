using CampusEats.Features.Orders;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Kitchen;

public class GetPendingOrdersHandler(CampusEatsContext context)
{
    public async Task<List<OrderResponse>> Handle(GetPendingOrdersRequest request)
    {
        var orders = await context.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Pending || 
                       o.Status == OrderStatus.Preparing || 
                       o.Status == OrderStatus.Ready)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(order => new OrderResponse(
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
        )).ToList();
    }
}
