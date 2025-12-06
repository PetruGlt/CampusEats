using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Kitchen;
using CampusEats.Persistence;
using FluentAssertions;

// Add aliases to disambiguate the Order entity from same-named test namespaces
using OrderEntity = CampusEats.Features.Orders.Order;
using OrderStatus = CampusEats.Features.Orders.OrderStatus;

namespace CampusEats.Tests.Features.Kitchen;

public class GetKitchenDashboardHandlerTests
{
    // Define a record for the request if not globally available in tests
    // public record GetKitchenDashboardRequest(); 

    private readonly DbContextOptions<CampusEatsContext> _options;

    public GetKitchenDashboardHandlerTests()
    {
        _options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;
    }

    private CampusEatsContext CreateContext() => new CampusEatsContext(_options);

    [Fact]
    public async Task GivenNoOrders_WhenHandleCalled_ThenShouldReturnZeroCountsAndDefaultAverageTime()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetKitchenDashboardHandler(context);

        // Act
        var result = await handler.Handle(new GetKitchenDashboardRequest());

        // Assert
        result.PendingOrdersCount.Should().Be(0);
        result.PreparingOrdersCount.Should().Be(0);
        result.ReadyOrdersCount.Should().Be(0);
        result.OldestPendingOrderId.Should().BeNull();
        result.AveragePreparationTimeMinutes.Should().Be(15); // Default value
        result.EstimatedCompletionTime.Should().BeNull();
    }

    [Fact]
    public async Task GivenMixedOrderStatuses_WhenHandleCalled_ThenShouldReturnCorrectStatusCounts()
    {
        // Arrange
        using var context = CreateContext();
        var orders = new List<OrderEntity>
        {
            new() { Id = Guid.NewGuid(), Status = OrderStatus.Pending },
            new() { Id = Guid.NewGuid(), Status = OrderStatus.Pending },
            new() { Id = Guid.NewGuid(), Status = OrderStatus.Preparing },
            new() { Id = Guid.NewGuid(), Status = OrderStatus.Ready },
            new() { Id = Guid.NewGuid(), Status = OrderStatus.Completed }, // Should be ignored in counts
            new() { Id = Guid.NewGuid(), Status = OrderStatus.Cancelled }  // Should be ignored in counts
        };
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        var handler = new GetKitchenDashboardHandler(context);

        // Act
        var result = await handler.Handle(new GetKitchenDashboardRequest());

        // Assert
        result.PendingOrdersCount.Should().Be(2);
        result.PreparingOrdersCount.Should().Be(1);
        result.ReadyOrdersCount.Should().Be(1);
    }

    [Fact]
    public async Task GivenMultiplePendingOrders_WhenHandleCalled_ThenShouldReturnOldestPendingOrder()
    {
        // Arrange
        using var context = CreateContext();
        var now = DateTime.UtcNow;
        
        var newerOrder = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending, CreatedAt = now };
        var olderOrder = new OrderEntity { Id = Guid.NewGuid(), Status = OrderStatus.Pending, CreatedAt = now.AddMinutes(-30) };
        
        context.Orders.AddRange(newerOrder, olderOrder);
        await context.SaveChangesAsync();

        var handler = new GetKitchenDashboardHandler(context);

        // Act
        var result = await handler.Handle(new GetKitchenDashboardRequest());

        // Assert
        result.OldestPendingOrderId.Should().Be(olderOrder.Id);
        result.OldestPendingOrderTime.Should().Be(olderOrder.CreatedAt);
    }

    [Fact]
    public async Task GivenCompletedOrders_WhenHandleCalled_ThenShouldCalculateCorrectAverageTime()
    {
        // Arrange
        using var context = CreateContext();
        var baseTime = DateTime.UtcNow;

        var completedOrders = new List<OrderEntity>
        {
            // Took 10 minutes
            new() { 
                Status = OrderStatus.Completed, 
                CreatedAt = baseTime, 
                UpdatedAt = baseTime.AddMinutes(10) 
            },
            // Took 20 minutes
            new() { 
                Status = OrderStatus.Completed, 
                CreatedAt = baseTime, 
                UpdatedAt = baseTime.AddMinutes(20) 
            },
            // Pending order (should not affect average calc)
            new() { 
                Status = OrderStatus.Pending,
                CreatedAt = baseTime 
            } 
        };
        context.Orders.AddRange(completedOrders);
        await context.SaveChangesAsync();

        var handler = new GetKitchenDashboardHandler(context);

        // Act
        var result = await handler.Handle(new GetKitchenDashboardRequest());

        // Assert
        // (10 + 20) / 2 = 15 minutes
        result.AveragePreparationTimeMinutes.Should().Be(15);
    }

    [Fact]
    public async Task GivenPendingOrderAndCalculatedAverage_WhenHandleCalled_ThenShouldReturnCorrectEstimatedCompletionTime()
    {
        // Arrange
        using var context = CreateContext();
        var now = DateTime.UtcNow;

        // 1. Setup history to force a specific average (e.g., 20 mins)
        var historyOrder = new OrderEntity 
        { 
            Status = OrderStatus.Completed, 
            CreatedAt = now.AddHours(-2), 
            UpdatedAt = now.AddHours(-2).AddMinutes(20) // Took 20 mins
        };

        // 2. Setup a current pending order created 5 mins ago
        var pendingOrder = new OrderEntity 
        { 
            Id = Guid.NewGuid(),
            Status = OrderStatus.Pending, 
            CreatedAt = now.AddMinutes(-5) 
        };

        context.Orders.AddRange(historyOrder, pendingOrder);
        await context.SaveChangesAsync();

        var handler = new GetKitchenDashboardHandler(context);

        // Act
        var result = await handler.Handle(new GetKitchenDashboardRequest());

        // Assert
        result.AveragePreparationTimeMinutes.Should().Be(20);
        
        // Est Completion = Pending.CreatedAt + AvgTime
        // (Now - 5min) + 20min = Now + 15min
        var expectedTime = pendingOrder.CreatedAt.AddMinutes(20);
        
        result.EstimatedCompletionTime.Should().Be(expectedTime);
    }
}