using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;

public class GetOrderStatisticsHandler(CampusEatsContext context)
{
    public async Task<OrderStatisticsResponse> Handle(GetOrderStatisticsRequest request)
    {
        var orders = await context.Orders.ToListAsync();
        
        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var statusCounts = orders
            .GroupBy(o => o.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var todayOrders = orders.Count(o => o.CreatedAt.Date == DateTime.UtcNow.Date);
        var todayRevenue = orders
            .Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date)
            .Sum(o => o.TotalAmount);

        return new OrderStatisticsResponse(
            totalOrders,
            totalRevenue,
            averageOrderValue,
            statusCounts,
            todayOrders,
            todayRevenue
        );
    }
}

public record OrderStatisticsResponse(
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    Dictionary<string, int> OrdersByStatus,
    int TodayOrders,
    decimal TodayRevenue
);
