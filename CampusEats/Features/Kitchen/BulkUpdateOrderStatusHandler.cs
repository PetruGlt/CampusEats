using CampusEats.Exceptions;
using CampusEats.Features.Orders;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Kitchen;

public class BulkUpdateOrderStatusHandler(CampusEatsContext context)
{
    public async Task<BulkUpdateOrderStatusResponse> Handle(BulkUpdateOrderStatusRequest request)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
        {
            throw new InvalidOrderStatusException($"Invalid status: {request.Status}");
        }

        var orders = await context.Orders
            .Where(o => request.OrderIds.Contains(o.Id))
            .ToListAsync();

        if (orders.Count != request.OrderIds.Count)
        {
            var foundIds = orders.Select(o => o.Id).ToList();
            var missingIds = request.OrderIds.Except(foundIds).ToList();
            throw new OrderNotFoundException(missingIds.First());
        }

        var updatedOrders = new List<Guid>();
        var failedOrders = new List<BulkUpdateFailure>();

        foreach (var order in orders)
        {
            try
            {
                ValidateStatusTransition(order.Status, newStatus);
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;
                updatedOrders.Add(order.Id);
            }
            catch (InvalidOrderStatusException ex)
            {
                failedOrders.Add(new BulkUpdateFailure(order.Id, ex.Message));
            }
        }

        await context.SaveChangesAsync();

        return new BulkUpdateOrderStatusResponse(
            updatedOrders.Count,
            failedOrders.Count,
            updatedOrders,
            failedOrders
        );
    }

    private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
        {
            { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.Cancelled } },
            { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.Ready, OrderStatus.Cancelled } },
            { OrderStatus.Ready, new List<OrderStatus> { OrderStatus.Completed } },
            { OrderStatus.Completed, new List<OrderStatus>() },
            { OrderStatus.Cancelled, new List<OrderStatus>() }
        };

        if (!validTransitions[currentStatus].Contains(newStatus))
        {
            throw new InvalidOrderStatusException(
                $"Cannot transition from {currentStatus} to {newStatus}");
        }
    }
}

public record BulkUpdateOrderStatusResponse(
    int SuccessCount,
    int FailureCount,
    List<Guid> UpdatedOrderIds,
    List<BulkUpdateFailure> Failures
);

public record BulkUpdateFailure(Guid OrderId, string Reason);
