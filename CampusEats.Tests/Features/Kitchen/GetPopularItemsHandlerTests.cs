using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using CampusEats.Features.Kitchen;
using CampusEats.Persistence;
using FluentAssertions;

// Disambiguate OrderItem type
using OrderItemEntity = CampusEats.Features.Orders.OrderItem;

namespace CampusEats.Tests.Features.Kitchen;

public class GetPopularItemsHandlerTests
{
    // Record definition for reference if not globally available
    // public record GetPopularItemsRequest(int? TopN);

    private readonly DbContextOptions<CampusEatsContext> _options;

    public GetPopularItemsHandlerTests()
    {
        _options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private CampusEatsContext CreateContext() => new CampusEatsContext(_options);

    [Fact]
    public async Task GivenMultipleOrdersWithSameItems_WhenHandleCalled_ThenShouldAggregateQuantitiesAndRevenue()
    {
        // Arrange
        using var context = CreateContext();
        var burgerId = Guid.NewGuid();
        var friesId = Guid.NewGuid();

        var items = new List<OrderItemEntity>
        {
            // 3 Burgers total (2 orders)
            new() { MenuItemId = burgerId, MenuItemName = "Burger", Quantity = 2, Price = 10m },
            new() { MenuItemId = burgerId, MenuItemName = "Burger", Quantity = 1, Price = 10m },
            
            // 5 Fries total (1 order)
            new() { MenuItemId = friesId, MenuItemName = "Fries", Quantity = 5, Price = 5m }
        };

        context.OrderItems.AddRange(items);
        await context.SaveChangesAsync();

        var handler = new GetPopularItemsHandler(context);
        var request = new GetPopularItemsRequest(10); // Top 10

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.Should().HaveCount(2);

        // Verify Fries (sold 5)
        var friesStats = result.First(x => x.MenuItemName == "Fries");
        friesStats.TotalQuantitySold.Should().Be(5);
        friesStats.TimesOrdered.Should().Be(1); // Appears in 1 row
        friesStats.TotalRevenue.Should().Be(25m); // 5 * 5.00

        // Verify Burger (sold 3)
        var burgerStats = result.First(x => x.MenuItemName == "Burger");
        burgerStats.TotalQuantitySold.Should().Be(3);
        burgerStats.TimesOrdered.Should().Be(2); // Appears in 2 rows
        burgerStats.TotalRevenue.Should().Be(30m); // 3 * 10.00
    }

    [Fact]
    public async Task GivenManyItems_WhenHandleCalledWithTopN_ThenShouldLimitResultsAndSortByPopularity()
    {
        // Arrange
        using var context = CreateContext();
        
        // Create 3 items with different popularity
        // Item A: Sold 10
        // Item B: Sold 20 (Most popular)
        // Item C: Sold 5  (Least popular)
        var items = new List<OrderItemEntity>
        {
            new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Item A", Quantity = 10, Price = 1m },
            new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Item B", Quantity = 20, Price = 1m },
            new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Item C", Quantity = 5, Price = 1m }
        };

        context.OrderItems.AddRange(items);
        await context.SaveChangesAsync();

        var handler = new GetPopularItemsHandler(context);
        
        // Request Top 2 only
        var request = new GetPopularItemsRequest(2);

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.Should().HaveCount(2); // Should filter out Item C
        
        // Should be ordered descending by Quantity
        result[0].MenuItemName.Should().Be("Item B"); // 20 sold
        result[1].MenuItemName.Should().Be("Item A"); // 10 sold
    }

    [Fact]
    public async Task GivenNoItemsInDatabase_WhenHandleCalled_ThenShouldReturnEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetPopularItemsHandler(context);

        // Act
        var result = await handler.Handle(new GetPopularItemsRequest(5));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GivenRequestWithNullTopN_WhenHandleCalled_ThenShouldDefaultToTenRecords()
    {
        // Arrange
        using var context = CreateContext();
        
        // Create 15 distinct items
        var items = Enumerable.Range(1, 15).Select(i => 
            new OrderItemEntity 
            { 
                MenuItemId = Guid.NewGuid(), 
                MenuItemName = $"Item {i}", 
                Quantity = i, // Higher index = higher quantity 
                Price = 1m 
            }
        );

        context.OrderItems.AddRange(items);
        await context.SaveChangesAsync();

        var handler = new GetPopularItemsHandler(context);
        
        // Null TopN
        var request = new GetPopularItemsRequest(null);

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.Should().HaveCount(10); // Default limit
        result.First().MenuItemName.Should().Be("Item 15"); // Most popular
        result.Last().MenuItemName.Should().Be("Item 6");   // 10th most popular
    }
}