using CampusEats.Features.Orders;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Kitchen;

public class GetKitchenDashboardHandler(CampusEatsContext context)
{
    public async Task<KitchenDashboardResponse> Handle(GetKitchenDashboardRequest request)
    {
        var activeOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Pending || 
                       o.Status == OrderStatus.Preparing || 
                       o.Status == OrderStatus.Ready)
            .ToListAsync();

        var pendingCount = activeOrders.Count(o => o.Status == OrderStatus.Pending);
        var preparingCount = activeOrders.Count(o => o.Status == OrderStatus.Preparing);
        var readyCount = activeOrders.Count(o => o.Status == OrderStatus.Ready);

        var oldestPendingOrder = activeOrders
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderBy(o => o.CreatedAt)
            .FirstOrDefault();

        var averagePreparationMinutes = await CalculateAveragePreparationTime();

        var estimatedCompletionTime = oldestPendingOrder != null
            ? oldestPendingOrder.CreatedAt.AddMinutes(averagePreparationMinutes)
            : (DateTime?)null;

        return new KitchenDashboardResponse(
            pendingCount,
            preparingCount,
            readyCount,
            oldestPendingOrder?.Id,
            oldestPendingOrder?.CreatedAt,
            averagePreparationMinutes,
            estimatedCompletionTime
        );
    }

    private async Task<double> CalculateAveragePreparationTime()
    {
        var completedOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.UpdatedAt.HasValue)
            .ToListAsync();

        if (!completedOrders.Any())
            return 15; // Default 15 minutes if no data

        var avgMinutes = completedOrders
            .Select(o => (o.UpdatedAt!.Value - o.CreatedAt).TotalMinutes)
            .Average();

        return avgMinutes;
    }
}

public record KitchenDashboardResponse(
    int PendingOrdersCount,
    int PreparingOrdersCount,
    int ReadyOrdersCount,
    Guid? OldestPendingOrderId,
    DateTime? OldestPendingOrderTime,
    double AveragePreparationTimeMinutes,
    DateTime? EstimatedCompletionTime
);
