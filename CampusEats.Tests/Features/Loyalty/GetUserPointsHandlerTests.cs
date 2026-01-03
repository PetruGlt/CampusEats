using CampusEats.Features.Loyalty;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using CampusEats.Persistence;

namespace CampusEats.Tests.Features.Loyalty;

public class GetUserPointsHandlerTests : IDisposable
{
    private readonly DbContextOptions<CampusEatsContext> _options;

    public GetUserPointsHandlerTests()
    {
        _options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private CampusEatsContext CreateContext() => new CampusEatsContext(_options);

    public void Dispose()
    {
        // Nothing special for now - in-memory DB per test is isolated by unique name
    }

    [Fact]
    public async Task GivenUserWithLoyaltyRecord_WhenHandleCalled_ThenShouldReturnCorrectPoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedPoints = 150;
        var lastUpdate = DateTime.UtcNow;

        await using var context = CreateContext();

        var mockLoyalty = new UserLoyalty
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Points = expectedPoints,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = lastUpdate
        };

        context.UserLoyalties.Add(mockLoyalty);
        await context.SaveChangesAsync();

        var loyaltyService = new LoyaltyService(context);
        var handler = new GetUserPointsHandler(loyaltyService);

        var request = new GetUserPointsRequest(userId);

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.UserId.Should().Be(userId);
        result.Points.Should().Be(expectedPoints);
        result.LastUpdated.Should().Be(lastUpdate);
    }

    [Fact]
    public async Task GivenUserWithNoHistory_WhenHandleCalled_ThenShouldReturnZeroPoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await using var context = CreateContext();

        var loyaltyService = new LoyaltyService(context);
        var handler = new GetUserPointsHandler(loyaltyService);

        var request = new GetUserPointsRequest(userId);

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.UserId.Should().Be(userId);
        result.Points.Should().Be(0);
        result.LastUpdated.Should().BeNull();
    }
}