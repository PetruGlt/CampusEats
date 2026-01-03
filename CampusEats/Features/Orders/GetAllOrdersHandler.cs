using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;

public class GetAllOrdersHandler(CampusEatsContext context)
{
    public async Task<List<OrderResponse>> Handle(GetAllOrdersRequest request)
    {
        var query = context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        /*if (!string.IsNullOrEmpty(request.UserId))
        {
            query = query.Where(o => o.UserId == request.UserId);
        }*/

        query = query.Where(o => o.UserId == request.UserId);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
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
