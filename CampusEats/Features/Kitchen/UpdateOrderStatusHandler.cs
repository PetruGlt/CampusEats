using CampusEats.Exceptions;
using CampusEats.Features.Orders;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Kitchen;

public class UpdateOrderStatusHandler(CampusEatsContext context)
{
    public async Task<OrderResponse> Handle(UpdateOrderStatusRequest request)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id);

        if (order == null)
        {
            throw new OrderNotFoundException(request.Id);
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
        {
            throw new InvalidOrderStatusException($"Invalid status: {request.Status}");
        }

        // Validate status transition
        ValidateStatusTransition(order.Status, newStatus);

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return new OrderResponse(
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
