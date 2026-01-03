using CampusEats.Exceptions;
using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Orders;
using CampusEats.Persistence;

// Disambiguate order types from test namespaces
using OrderEntity = CampusEats.Features.Orders.Order;
using OrderItemEntity = CampusEats.Features.Orders.OrderItem;
using OrderStatus = CampusEats.Features.Orders.OrderStatus;

namespace CampusEats.Tests.Features.Order;

public class CancelOrderHandlerTests
{
    private CampusEatsContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CampusEatsContext(options);
    }

    [Fact]
    public async Task GivenExistingPendingOrder_WhenHandleCalled_ThenReturnCancelledOrderResponse()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid();
        
        using (var context = GetInMemoryContext(dbName))
        {
            context.Orders.Add(new OrderEntity
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                UserId = Guid.NewGuid(),
                TotalAmount = 100,
                Items = new List<OrderItemEntity> 
                { 
                    new OrderItemEntity { MenuItemName = "Pizza", Price = 50, Quantity = 2 } 
                }
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new CancelOrderHandler(context);
            var request = new CancelOrderRequest(orderId);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Equal(OrderStatus.Cancelled.ToString(), result.Status);
            Assert.NotNull(result.UpdatedAt); 
        }
    }

    [Fact]
    public async Task GivenNonExistentOrder_WhenHandleCalled_ThenThrowOrderNotFoundException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new CancelOrderHandler(context);
            var request = new CancelOrderRequest(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<OrderNotFoundException>(() => handler.Handle(request));
        }
    }

    [Fact]
    public async Task GivenCompletedOrder_WhenHandleCalled_ThenThrowInvalidOrderStatusException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid();

        using (var context = GetInMemoryContext(dbName))
        {
            context.Orders.Add(new OrderEntity
            {
                Id = orderId,
                Status = OrderStatus.Completed, 
                UserId = Guid.NewGuid()
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new CancelOrderHandler(context);
            var request = new CancelOrderRequest(orderId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOrderStatusException>(() => handler.Handle(request));
            Assert.Contains("Cannot cancel order", exception.Message);
        }
    }
}