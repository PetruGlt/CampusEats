using CampusEats.Features.Payment;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CampusEats.Tests.Features.Payment;

public class GetPaymentHistoryHandlerTests
{
    private readonly Guid userId1 = Guid.NewGuid();
    private CampusEatsContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CampusEatsContext(options);
    }

    private async Task SeedDatabase(CampusEatsContext context)
    {
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        context.Payments.AddRange(
            new global::Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId1,
                Amount = 1000, // 10.00 USD
                Status = PaymentStatus.Succeeded,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new global::Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId2,
                Amount = 2500, // 25.00 USD
                Status = PaymentStatus.Failed,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new global::Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId3,
                Amount = 5000,
                Status = PaymentStatus.Succeeded,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GivenPaymentsExist_WhenHandleCalledWithNoFilters_ThenReturnAllPayments()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new GetPaymentHistoryHandler(context);
            var request = new GetPaymentHistoryRequest(null, null, null, null);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Equal(3, result.Count);
        }
    }

    [Fact]
    public async Task GivenPaymentsExist_WhenHandleCalledWithUserId_ThenReturnUserPayments()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new GetPaymentHistoryHandler(context);
            var request = new GetPaymentHistoryRequest(userId1.ToString(), null, null, null);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.All(result, p => Assert.Equal(Guid.NewGuid(), p.UserId));
        }
    }

    [Fact]
    public async Task GivenPaymentsExist_WhenHandleCalledWithStatusFilter_ThenReturnMatchingStatus()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new GetPaymentHistoryHandler(context);
            var request = new GetPaymentHistoryRequest(null, null, null, PaymentStatus.Failed);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Single(result);
            Assert.Equal("Failed", result.First().Status);
        }
    }

    [Fact]
    public async Task GivenPaymentsExist_WhenHandleCalledWithDateRange_ThenReturnPaymentsInInterval()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetInMemoryContext(dbName))
        {
            await SeedDatabase(context);
            var handler = new GetPaymentHistoryHandler(context);
            // Cautam plati de acum 6 zile pana acum 3 zile
            var request = new GetPaymentHistoryRequest(null, DateTime.UtcNow.AddDays(-6), DateTime.UtcNow.AddDays(-3), null);

            // Act
            var result = await handler.Handle(request);

            // Assert
            Assert.Single(result);
            // Doar plata de acum 5 zile se incadreaza
            Assert.Equal(1000, result.First().Amount); 
        }
    }
}