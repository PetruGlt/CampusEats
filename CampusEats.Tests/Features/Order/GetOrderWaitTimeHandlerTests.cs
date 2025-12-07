using CampusEats.Exceptions;
using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Orders;
using CampusEats.Persistence;

using OrderEntity = CampusEats.Features.Orders.Order;
using OrderStatus = CampusEats.Features.Orders.OrderStatus;

namespace CampusEats.Tests.Features.Order;

public class GetOrderWaitTimeHandlerTests
{
    private CampusEatsContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CampusEatsContext(options);
    }

    [Fact]
    public async Task GivenNonExistentOrder_WhenHandleCalled_ThenThrowOrderNotFoundException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetInMemoryContext(dbName);
        var handler = new GetOrderWaitTimeHandler(context);
        
        await Assert.ThrowsAsync<OrderNotFoundException>(() => 
            handler.Handle(new GetOrderWaitTimeRequest(Guid.NewGuid())));
    }

    [Fact]
    public async Task GivenCompletedOrder_WhenHandleCalled_ThenReturnZeroWaitTime()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid();
        using (var context = GetInMemoryContext(dbName))
        {
            context.Orders.Add(new OrderEntity { Id = orderId, Status = OrderStatus.Completed });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new GetOrderWaitTimeHandler(context);
            var result = await handler.Handle(new GetOrderWaitTimeRequest(orderId));

            Assert.Equal(0, result.EstimatedWaitMinutes);
            Assert.Contains("already completed", result.Message);
        }
    }

    [Fact]
    public async Task GivenPendingOrderWithNoHistory_WhenHandleCalled_ThenUseDefaultTime()
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
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem> { new OrderItem() } // 1 Item
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new GetOrderWaitTimeHandler(context);
            var result = await handler.Handle(new GetOrderWaitTimeRequest(orderId));

            // Formula: (0 orders ahead + 1) * 15 min avg * 1.0 complexity = 15
            Assert.Equal(15, result.EstimatedWaitMinutes);
        }
    }

    [Fact]
    public async Task GivenPendingOrderWithQueueAndComplexity_WhenHandleCalled_ThenCalculateCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var orderId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        using (var context = GetInMemoryContext(dbName))
        {
            // 1. Comanda finalizata (pentru a stabili media de timp)
            // A durat 10 minute (Updated - Created)
            context.Orders.Add(new OrderEntity 
            { 
                Id = Guid.NewGuid(), 
                Status = OrderStatus.Completed, 
                CreatedAt = now.AddMinutes(-20),
                UpdatedAt = now.AddMinutes(-10) 
            });

            // 2. Comanda in fata la coada (Pending, creata inainte de comanda noastra)
            context.Orders.Add(new OrderEntity 
            { 
                Id = Guid.NewGuid(), 
                Status = OrderStatus.Preparing, 
                CreatedAt = now.AddMinutes(-5) 
            });

            // 3. Comanda noastra (Pending, 3 iteme)
            context.Orders.Add(new OrderEntity 
            { 
                Id = orderId, 
                Status = OrderStatus.Pending, 
                CreatedAt = now,
                Items = new List<OrderItem> 
                { 
                    new OrderItem(), new OrderItem(), new OrderItem() // 3 items
                }
            });

            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new GetOrderWaitTimeHandler(context);
            var result = await handler.Handle(new GetOrderWaitTimeRequest(orderId));

            // CALCULE:
            // Orders Ahead = 1
            // Avg Time = 10 minute (din comanda finalizata)
            // Complexity = 1 + (3 items - 1) * 0.1 = 1.2
            // Formula: (1 + 1) * 10 * 1.2 = 24 minute
            
            Assert.Equal(24, result.EstimatedWaitMinutes);
        }
    }
}