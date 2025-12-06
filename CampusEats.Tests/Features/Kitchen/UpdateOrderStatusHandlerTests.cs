using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Kitchen;
using CampusEats.Persistence;
using CampusEats.Exceptions;
using FluentAssertions;

// Disambiguate Order types (tests may have conflicting namespaces)
using OrderEntity = CampusEats.Features.Orders.Order;
using OrderItemEntity = CampusEats.Features.Orders.OrderItem;
using OrderStatus = CampusEats.Features.Orders.OrderStatus;

namespace CampusEats.Tests.Features.Kitchen;

public class UpdateOrderStatusHandlerTests
{
    private readonly DbContextOptions<CampusEatsContext> _options;

    public UpdateOrderStatusHandlerTests()
    {
        _options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private CampusEatsContext CreateContext() => new CampusEatsContext(_options);

    [Fact]
    public async Task GivenValidTransition_WhenHandleCalled_ThenShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var originalTime = DateTime.UtcNow.AddHours(-1);

        var order = new OrderEntity
        {
            Id = orderId,
            Status = OrderStatus.Pending,
            CreatedAt = originalTime,
            UpdatedAt = originalTime,
            Items = new List<OrderItemEntity>()
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusHandler(context);
        var request = new UpdateOrderStatusRequest(orderId, "Preparing");

        // Act
        var result = await handler.Handle(request);

        // Assert response
        result.Status.Should().Be("Preparing");
        result.UpdatedAt.Should().BeAfter(originalTime);

        // Assert Database persistence
        using var verifyContext = CreateContext();
        var dbOrder = await verifyContext.Orders.FindAsync(orderId);
        dbOrder!.Status.Should().Be(OrderStatus.Preparing);
    }

    [Fact]
    public async Task GivenNonExistentId_WhenHandleCalled_ThenShouldThrowOrderNotFoundException()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new UpdateOrderStatusHandler(context);
        var request = new UpdateOrderStatusRequest(Guid.NewGuid(), "Preparing");

        // Act & Assert
        await Assert.ThrowsAsync<OrderNotFoundException>(() => handler.Handle(request));
    }

    [Fact]
    public async Task GivenInvalidStatusString_WhenHandleCalled_ThenShouldThrowInvalidOrderStatusException()
    {
        // Arrange
        using var context = CreateContext();
        var order = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusHandler(context);
        var request = new UpdateOrderStatusRequest(order.Id, "SuperDuperDone");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOrderStatusException>(() => handler.Handle(request));
        ex.Message.Should().Contain("Invalid status");
    }

    [Theory]
    // Cannot skip steps
    [InlineData(OrderStatus.Pending, "Completed")]
    // Cannot go backwards
    [InlineData(OrderStatus.Ready, "Preparing")]
    // Cannot modify final states
    [InlineData(OrderStatus.Completed, "Pending")]
    [InlineData(OrderStatus.Cancelled, "Ready")]
    public async Task GivenInvalidStateTransition_WhenHandleCalled_ThenShouldThrowInvalidOrderStatusException(OrderStatus current, string target)
    {
        // Arrange
        using var context = CreateContext();
        var order = new OrderEntity { Id = Guid.NewGuid(), Status = current };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusHandler(context);
        var request = new UpdateOrderStatusRequest(order.Id, target);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOrderStatusException>(() => handler.Handle(request));
        ex.Message.Should().Contain($"Cannot transition from {current}");
    }

    [Fact]
    public async Task GivenCaseInsensitiveStatus_WhenHandleCalled_ThenShouldParseSuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var order = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusHandler(context);
        var request = new UpdateOrderStatusRequest(order.Id, "preparing");

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.Status.Should().Be("Preparing");
    }
}
