using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Orders;
using CampusEats.Persistence;

using OrderEntity = CampusEats.Features.Orders.Order;
using OrderStatus = CampusEats.Features.Orders.OrderStatus;

namespace CampusEats.Tests.Features.Order;

public class GetOrderStatisticsHandlerTests
{
    private CampusEatsContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CampusEatsContext(options);
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenHandleCalled_ThenReturnZeros()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetInMemoryContext(dbName);
        var handler = new GetOrderStatisticsHandler(context);

        var result = await handler.Handle(new GetOrderStatisticsRequest());

        Assert.Equal(0, result.TotalOrders);
        Assert.Equal(0, result.TotalRevenue);
        Assert.Empty(result.OrdersByStatus);
    }

    [Fact]
    public async Task GivenOrders_WhenHandleCalled_ThenReturnCorrectAggregates()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var today = DateTime.UtcNow; 

        using (var context = GetInMemoryContext(dbName))
        {
            context.Orders.AddRange(
                // Comanda de azi, Completed, 100 RON
                new OrderEntity 
                { 
                    Id = Guid.NewGuid(), 
                    Status = OrderStatus.Completed, 
                    TotalAmount = 100, 
                    CreatedAt = today 
                },
                // Comanda de ieri, Pending, 50 RON
                new OrderEntity 
                { 
                    Id = Guid.NewGuid(), 
                    Status = OrderStatus.Pending, 
                    TotalAmount = 50, 
                    CreatedAt = today.AddDays(-1) 
                }
            );
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new GetOrderStatisticsHandler(context);
            
            // Act
            var result = await handler.Handle(new GetOrderStatisticsRequest());

            // Assert
            Assert.Equal(2, result.TotalOrders); // 1 azi + 1 ieri
            Assert.Equal(150, result.TotalRevenue); // 100 + 50
            Assert.Equal(75, result.AverageOrderValue); // 150 / 2
            
            // Verificari pentru "Today"
            Assert.Equal(1, result.TodayOrders);
            Assert.Equal(100, result.TodayRevenue);

            // Verificari dictionar status
            Assert.True(result.OrdersByStatus.ContainsKey("Completed"));
            Assert.Equal(1, result.OrdersByStatus["Completed"]);
            Assert.Equal(1, result.OrdersByStatus["Pending"]);
        }
    }
}