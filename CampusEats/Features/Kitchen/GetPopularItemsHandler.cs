using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Kitchen;

public class GetPopularItemsHandler(CampusEatsContext context)
{
    public async Task<List<PopularItemResponse>> Handle(GetPopularItemsRequest request)
    {
        var topN = request.TopN ?? 10;

        // Do aggregates in a form EF Core can translate, then materialize and map in-memory
        var grouped = await context.OrderItems
            .GroupBy(oi => new { oi.MenuItemId, oi.MenuItemName })
            .Select(g => new
            {
                MenuItemId = g.Key.MenuItemId,
                MenuItemName = g.Key.MenuItemName,
                TotalQuantitySold = g.Sum(oi => oi.Quantity),
                TimesOrdered = g.Count(),
                TotalRevenue = g.Sum(oi => oi.Price * (decimal)oi.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(topN)
            .ToListAsync();

        var popularItems = grouped
            .Select(g => new PopularItemResponse(
                g.MenuItemId,
                g.MenuItemName,
                g.TotalQuantitySold,
                g.TimesOrdered,
                g.TotalRevenue
            ))
            .ToList();

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
