using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Kitchen;
using CampusEats.Persistence;
using CampusEats.Exceptions;
using FluentAssertions;

// Disambiguate the Order type (tests have a similar namespace) by using aliases
using OrderEntity = CampusEats.Features.Orders.Order;
using OrderStatus = CampusEats.Features.Orders.OrderStatus;

namespace CampusEats.Tests.Features.Kitchen;

public class BulkUpdateOrderStatusHandlerTests
{
    private readonly DbContextOptions<CampusEatsContext> _options;

    public BulkUpdateOrderStatusHandlerTests()
    {
        // Setup a fresh In-Memory database for every test run
        _options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private CampusEatsContext CreateContext() => new CampusEatsContext(_options);

    [Fact]
    public async Task GivenValidOrdersAndStatus_WhenHandleCalled_ThenShouldReturnSuccessAndPersistChanges()
    {
        // Arrange
        using var context = CreateContext();
        var order1 = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        var order2 = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var handler = new BulkUpdateOrderStatusHandler(context);
        var request = new BulkUpdateOrderStatusRequest(
            new List<Guid> { order1.Id, order2.Id },
            "Preparing"
        );

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(0);
        result.UpdatedOrderIds.Should().Contain(new[] { order1.Id, order2.Id });

        // Verify Persistence
        using var verifyContext = CreateContext();
        var dbOrder1 = await verifyContext.Orders.FindAsync(order1.Id);
        dbOrder1!.Status.Should().Be(OrderStatus.Preparing);
        dbOrder1.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GivenInvalidStatusString_WhenHandleCalled_ThenShouldThrowInvalidOrderStatusException()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new BulkUpdateOrderStatusHandler(context);
        var request = new BulkUpdateOrderStatusRequest(
            new List<Guid> { Guid.NewGuid() },
            "NotARealStatus"
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOrderStatusException>(() => handler.Handle(request));
    }

    [Fact]
    public async Task GivenNonExistentOrderIds_WhenHandleCalled_ThenShouldThrowOrderNotFoundException()
    {
        // Arrange
        using var context = CreateContext();
        // Database is empty

        var handler = new BulkUpdateOrderStatusHandler(context);
        var missingId = Guid.NewGuid();
        var request = new BulkUpdateOrderStatusRequest(
            new List<Guid> { missingId },
            "Preparing"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OrderNotFoundException>(() => handler.Handle(request));

        // Optional: Check if the exception contains the ID (depending on your Exception implementation)
        // exception.Message.Should().Contain(missingId.ToString());
    }

    [Fact]
    public async Task GivenOrdersWithInvalidTransitions_WhenHandleCalled_ThenShouldReturnFailuresAndNotUpdateOrders()
    {
        // Arrange
        using var context = CreateContext();
        // 'Cancelled' cannot go to 'Ready' based on your logic
        var order = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Cancelled };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new BulkUpdateOrderStatusHandler(context);
        var request = new BulkUpdateOrderStatusRequest(
            new List<Guid> { order.Id },
            "Ready"
        );

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(1);
        result.Failures.First().Reason.Should().Contain("Cannot transition");

        // Verify Persistence (Should remain Cancelled)
        using var verifyContext = CreateContext();
        var dbOrder = await verifyContext.Orders.FindAsync(order.Id);
        dbOrder!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task GivenMixedValidAndInvalidTransitions_WhenHandleCalled_ThenShouldReturnPartialSuccess()
    {
        // Arrange
        using var context = CreateContext();
        var validOrder = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        var invalidOrder = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Cancelled };

        context.Orders.AddRange(validOrder, invalidOrder);
        await context.SaveChangesAsync();

        var handler = new BulkUpdateOrderStatusHandler(context);
        // Trying to set both to 'Preparing'
        // Pending -> Preparing (Valid)
        // Cancelled -> Preparing (Invalid)
        var request = new BulkUpdateOrderStatusRequest(
            new List<Guid> { validOrder.Id, invalidOrder.Id },
            "Preparing"
        );

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(1);

        result.UpdatedOrderIds.Should().Contain(validOrder.Id);
        result.Failures.Should().Contain(f => f.OrderId == invalidOrder.Id);

        // Verify Persistence
        using var verifyContext = CreateContext();
        var dbValid = await verifyContext.Orders.FindAsync(validOrder.Id);
        var dbInvalid = await verifyContext.Orders.FindAsync(invalidOrder.Id);

        dbValid!.Status.Should().Be(OrderStatus.Preparing); // Changed
        dbInvalid!.Status.Should().Be(OrderStatus.Cancelled); // Unchanged
    }
}