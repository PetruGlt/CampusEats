using CampusEats.Features.Orders;
using CampusEats.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CampusEats.Tests.Validators;

public class CreateOrderValidatorTests : IDisposable
{
    private CreateOrderValidator _sut;

    public CreateOrderValidatorTests()
    {
        _sut = CreateSUT();
    }

    [Fact]
    public void GivenEmptyUserId_WhenValidating_ThenShouldReturnError()
    {
        var order = new CreateOrderRequest(Guid.Empty , new List<OrderItemDto>
        {
            new (Guid.NewGuid(), 1, null)
        }, null);

        var result = _sut.TestValidate(order);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }
    
    [Fact]
    public void GivenEmptyOrderItemList_WhenValidating_ThenShouldReturnError()
    {
        var order = new CreateOrderRequest(Guid.NewGuid() , null, null);

        var result = _sut.TestValidate(order);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void GivenListOfInvalidOrderItems_WhenValidating_ThenShouldReturnErrors()
    {
        var order = new CreateOrderRequest(Guid.NewGuid() , new List<OrderItemDto>
        {
            new (Guid.Empty, 1, null), //Invalid MenuItemId
            new (Guid.NewGuid(), -1, null) //Invalid Quantity
        }, null);
        
        var result = _sut.TestValidate(order);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
    
    
    private CreateOrderValidator CreateSUT() => new();

    public void Dispose()
    {
        _sut = null;
    }
}