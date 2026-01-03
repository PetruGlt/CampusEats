using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;

public class SearchOrdersHandler(CampusEatsContext context)
{
    public async Task<List<OrderResponse>> Handle(SearchOrdersRequest request)
    {
        var query = context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var searchTerm = request.Query.ToLower();
            query = query.Where(o => 
                (o.Notes != null && o.Notes.ToLower().Contains(searchTerm)) ||
                o.Items.Any(i => i.MenuItemName.ToLower().Contains(searchTerm))
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<OrderStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(o => o.Status == statusEnum);
            }
        }

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
