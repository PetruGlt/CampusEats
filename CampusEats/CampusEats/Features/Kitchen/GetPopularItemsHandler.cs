using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Kitchen;

public class GetPopularItemsHandler(CampusEatsContext context)
{
    public async Task<List<PopularItemResponse>> Handle(GetPopularItemsRequest request)
    {
        var topN = request.TopN ?? 10;

        var popularItems = await context.OrderItems
            .GroupBy(oi => new { oi.MenuItemId, oi.MenuItemName })
            .Select(g => new PopularItemResponse(
                g.Key.MenuItemId,
                g.Key.MenuItemName,
                g.Sum(oi => oi.Quantity),
                g.Count(),
                g.Sum(oi => oi.Price * oi.Quantity)
            ))
            .OrderByDescending(item => item.TotalQuantitySold)
            .Take(topN)
            .ToListAsync();

        return popularItems;
    }
}

public record PopularItemResponse(
    Guid MenuItemId,
    string MenuItemName,
    int TotalQuantitySold,
    int TimesOrdered,
    decimal TotalRevenue
);
