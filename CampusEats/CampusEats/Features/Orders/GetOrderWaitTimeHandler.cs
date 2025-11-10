using CampusEats.Exceptions;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;

public class GetOrderWaitTimeHandler(CampusEatsContext context)
{
    public async Task<OrderWaitTimeResponse> Handle(GetOrderWaitTimeRequest request)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
        {
            return new OrderWaitTimeResponse(
                order.Id,
                order.Status.ToString(),
                0,
                null,
                "Order is already completed or cancelled"
            );
        }

        if (order.Status == OrderStatus.Ready)
        {
            return new OrderWaitTimeResponse(
                order.Id,
                order.Status.ToString(),
                0,
                DateTime.UtcNow,
                "Order is ready for pickup!"
            );
        }

        // Calculate position in queue
        var ordersAhead = await context.Orders
            .Where(o => (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Preparing) &&
                       o.CreatedAt < order.CreatedAt)
            .CountAsync();

        // Calculate average preparation time
        var completedOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.UpdatedAt.HasValue)
            .ToListAsync();

        double avgMinutesPerOrder = 15; // Default
        if (completedOrders.Any())
        {
            avgMinutesPerOrder = completedOrders
                .Select(o => (o.UpdatedAt!.Value - o.CreatedAt).TotalMinutes)
                .Average();
        }

        // Calculate complexity factor based on number of items
        var itemCount = order.Items.Count;
        var complexityMultiplier = 1 + (itemCount - 1) * 0.1; // 10% more time per additional item

        var estimatedMinutes = (ordersAhead + 1) * avgMinutesPerOrder * complexityMultiplier;
        var estimatedCompletionTime = DateTime.UtcNow.AddMinutes(estimatedMinutes);

        return new OrderWaitTimeResponse(
            order.Id,
            order.Status.ToString(),
            (int)Math.Ceiling(estimatedMinutes),
            estimatedCompletionTime,
            $"Approximately {(int)Math.Ceiling(estimatedMinutes)} minutes. {ordersAhead} order(s) ahead of you."
        );
    }
}

public record OrderWaitTimeResponse(
    Guid OrderId,
    string CurrentStatus,
    int EstimatedWaitMinutes,
    DateTime? EstimatedCompletionTime,
    string Message
);
