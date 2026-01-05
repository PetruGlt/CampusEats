
using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Orders;
using CampusEats.Persistence;

using OrderEntity = CampusEats.Features.Orders.Order;

namespace CampusEats.Tests.Features.Order;

public class GetOrderHistoryHandlerTests
{
    private CampusEatsContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CampusEatsContext(options);
    }

    [Fact]
    public async Task GivenOrdersExist_WhenHandleCalledWithNoFilters_ThenReturnAllOrders()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        using (var context = GetInMemoryContext(dbName))
        {
            context.Orders.AddRange(
                new OrderEntity { Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow },
                new OrderEntity { Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new GetOrderHistoryHandler(context);
            var request = new GetOrderHistoryRequest(null, null, userId);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Equal(2, result.Count);
        }
    }

    [Fact]
    public async Task GivenOrdersExist_WhenHandleCalledWithUserIdFilter_ThenReturnUserOrdersOnly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var targetUser = Guid.NewGuid();
        using (var context = GetInMemoryContext(dbName))
        {
            context.Orders.AddRange(
                new OrderEntity { Id = Guid.NewGuid(), UserId = targetUser, CreatedAt = DateTime.UtcNow },
                new OrderEntity { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new GetOrderHistoryHandler(context);
            var request = new GetOrderHistoryRequest(null, null, targetUser);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Single(result);
            Assert.Equal(targetUser, result.First().UserId);
        }
    }

    [Fact]
    public async Task GivenOrdersExist_WhenHandleCalledWithDateRange_ThenReturnOrdersInInterval()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var today = DateTime.UtcNow;
        using (var context = GetInMemoryContext(dbName))
        {
            context.Orders.AddRange(
                new OrderEntity { Id = Guid.NewGuid(), UserId = userId, CreatedAt = today.AddDays(-5) }, // Prea veche
                new OrderEntity { Id = Guid.NewGuid(), UserId = userId, CreatedAt = today.AddDays(-2) }, // Buna
                new OrderEntity { Id = Guid.NewGuid(), UserId = userId, CreatedAt = today.AddDays(1) }   // Prea noua
            );
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var handler = new GetOrderHistoryHandler(context);
            // Cautam comenzi intre acum 3 zile si acum 1 zi
            var request = new GetOrderHistoryRequest(today.AddDays(-3), today.AddDays(-1), userId);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Single(result);
            // Doar comanda de acum 2 zile ar trebui sa fie returnata
            Assert.Equal(today.AddDays(-2).Date, result.First().CreatedAt.Date);
        }
    }
}