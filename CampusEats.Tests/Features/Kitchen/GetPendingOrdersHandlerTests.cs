using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Kitchen;
using CampusEats.Persistence;
using FluentAssertions;

using OrderEntity = CampusEats.Features.Orders.Order;
using OrderItemEntity = CampusEats.Features.Orders.OrderItem;
using OrderStatus = CampusEats.Features.Orders.OrderStatus;

namespace CampusEats.Tests.Features.Kitchen;

public class GetPendingOrdersHandlerTests
{
    private readonly DbContextOptions<CampusEatsContext> _options;

    public GetPendingOrdersHandlerTests()
    {
        // Use a unique database name for each test to ensure isolation
        _options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private CampusEatsContext CreateContext() => new CampusEatsContext(_options);

    [Fact]
    public async Task GivenMixedOrderStatuses_WhenHandleCalled_ThenShouldReturnOnlyActiveOrders()
    {
        // Arrange
        using var context = CreateContext();
        
        // Active orders (Should be returned)
        var pending = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        var preparing = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Preparing };
        var ready = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Ready };
        
        // Inactive orders (Should be filtered out)
        var completed = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Completed };
        var cancelled = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Cancelled };

        context.Orders.AddRange(pending, completed, preparing, cancelled, ready);
        await context.SaveChangesAsync();

        var handler = new GetPendingOrdersHandler(context);

        // Act
        var result = await handler.Handle(new GetPendingOrdersRequest());

        // Assert
        result.Should().HaveCount(3);
        result.Select(o => o.Status).Should().Contain(new[] { "Pending", "Preparing", "Ready" });
        result.Select(o => o.Status).Should().NotContain(new[] { "Completed", "Cancelled" });
    }

    [Fact]
    public async Task GivenMultipleOrders_WhenHandleCalled_ThenShouldReturnSortedByCreatedAtAscending()
    {
        // Arrange
        using var context = CreateContext();
        var baseTime = DateTime.UtcNow;

        var oldOrder = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending, CreatedAt = baseTime.AddMinutes(-30) };
        var newOrder = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending, CreatedAt = baseTime };
        var midOrder = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending, CreatedAt = baseTime.AddMinutes(-15) };

        context.Orders.AddRange(newOrder, oldOrder, midOrder); // Added in random order
        await context.SaveChangesAsync();

        var handler = new GetPendingOrdersHandler(context);

        // Act
        var result = await handler.Handle(new GetPendingOrdersRequest());

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(oldOrder.Id); // Oldest first
        result[1].Id.Should().Be(midOrder.Id);
        result[2].Id.Should().Be(newOrder.Id); // Newest last
    }

    [Fact]
    public async Task GivenOrderWithItems_WhenHandleCalled_ThenShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var order = new OrderEntity
        {
            Id = orderId,
            Status = OrderStatus.Pending,
            TotalAmount = 45.50m,
            CreatedAt = DateTime.UtcNow,
            Notes = "Allergic to peanuts",
            Items = new List<OrderItemEntity>
            {
                new() 
                { 
                    Id = itemId, 
                    MenuItemId = Guid.NewGuid(),
                    MenuItemName = "Pizza", 
                    Price = 20.00m, 
                    Quantity = 2, 
                    SpecialInstructions = "Extra cheese" 
                }
            }
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new GetPendingOrdersHandler(context);

        // Act
        var result = await handler.Handle(new GetPendingOrdersRequest());

        // Assert
        var resultOrder = result.Single();
        
        // Check Order details
        resultOrder.Id.Should().Be(orderId);
        resultOrder.TotalAmount.Should().Be(45.50m);
        resultOrder.Status.Should().Be("Pending");
        resultOrder.Notes.Should().Be("Allergic to peanuts");

        // Check Item details
        resultOrder.Items.Should().HaveCount(1);
        var resultItem = resultOrder.Items.Single();
        resultItem.Id.Should().Be(itemId);
        resultItem.MenuItemName.Should().Be("Pizza");
        resultItem.Quantity.Should().Be(2);
        resultItem.SpecialInstructions.Should().Be("Extra cheese");
    }

    [Fact]
    public async Task GivenNoActiveOrders_WhenHandleCalled_ThenShouldReturnEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        // Only inactive orders exist
        context.Orders.Add(new OrderEntity { Status = OrderStatus.Completed });
        await context.SaveChangesAsync();

        var handler = new GetPendingOrdersHandler(context);

        // Act
        var result = await handler.Handle(new GetPendingOrdersRequest());

        // Assert
        result.Should().BeEmpty();
    }
}