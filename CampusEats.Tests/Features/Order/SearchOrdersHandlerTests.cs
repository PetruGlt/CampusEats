using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Orders;
using CampusEats.Persistence;

using OrderEntity = CampusEats.Features.Orders.Order;
using OrderStatus = CampusEats.Features.Orders.OrderStatus;

namespace CampusEats.Tests.Features.Order;

public class SearchOrdersHandlerTests
{
    private readonly Guid userId1 = Guid.NewGuid();
    private readonly Guid userId2 = Guid.NewGuid();
    private readonly Guid userId3 = Guid.NewGuid();
    private CampusEatsContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CampusEatsContext(options);
    }

    private async Task SeedDatabase(CampusEatsContext context)
    {
        context.Orders.AddRange(
            new OrderEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId1,
                Status = OrderStatus.Pending,
                Notes = $"Leave at door {userId1}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                Items = new List<OrderItem> 
                { 
                    new OrderItem { MenuItemName = "Burger", Price = 10, Quantity = 1 } 
                }
            },
            new OrderEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId2,
                Status = OrderStatus.Completed,
                Notes = null,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20),
                Items = new List<OrderItem> 
                { 
                    new OrderItem { MenuItemName = "Salad", Price = 15, Quantity = 1 } 
                }
            },
            new OrderEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId3,
                Status = OrderStatus.Cancelled,
                Notes = "Changed mind",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                Items = new List<OrderItem> 
                { 
                    new OrderItem { MenuItemName = "Pizza", Price = 20, Quantity = 1 } 
                }
            }
        );
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GivenOrdersExist_WhenHandleCalledWithEmptyRequest_ThenReturnAllOrdersOrderedByDate()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new SearchOrdersHandler(context);
            var request = new SearchOrdersRequest(null, null);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Equal(3, result.Count);
            // Verificăm ordinea descrescătoare (cel mai recent primul)
            Assert.True(result[0].CreatedAt > result[1].CreatedAt);
        }
    }

    [Fact]
    public async Task GivenOrdersExist_WhenHandleCalledWithUserIdQuery_ThenReturnMatchingOrders()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new SearchOrdersHandler(context);
            var request = new SearchOrdersRequest(userId1.ToString(), null); 

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Single(result);
            Assert.Equal(userId1, result.First().UserId);
        }
    }

    [Fact]
    public async Task GivenOrdersExist_WhenHandleCalledWithItemNameQuery_ThenReturnMatchingOrders()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new SearchOrdersHandler(context);
            var request = new SearchOrdersRequest("Burger", null);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Single(result);
            Assert.Contains(result.First().Items, i => i.MenuItemName == "Burger");
        }
    }

    [Fact]
    public async Task GivenOrdersExist_WhenHandleCalledWithStatusFilter_ThenReturnOnlyMatchingStatus()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new SearchOrdersHandler(context);
            var request = new SearchOrdersRequest(null, "Completed");

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Single(result);
            Assert.Equal("Completed", result.First().Status);
        }
    }

    [Fact]
    public async Task GivenOrdersExist_WhenHandleCalledWithInvalidStatus_ThenReturnAllMatchingQuery()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new SearchOrdersHandler(context);
            // Status invalid, ar trebui sa fie ignorat de Enum.TryParse
            var request = new SearchOrdersRequest(null, "InvalidStatusXYZ"); 

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Equal(3, result.Count); // Returneaza tot deoarece filtrul de status a eșuat
        }
    }
}