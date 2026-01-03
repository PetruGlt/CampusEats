using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;

public class GetOrderHistoryHandler(CampusEatsContext context)
{
    public async Task<List<OrderResponse>> Handle(GetOrderHistoryRequest request)
    {
        var query = context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.EndDate.Value);
        }

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
