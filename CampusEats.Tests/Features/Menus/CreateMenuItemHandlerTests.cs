using CampusEats.Features.Menu;
using CampusEats.Tests.Helpers;
using CampusEats.Validators;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace CampusEats.Tests.Features.Menus;

public class CreateMenuItemHandlerTests : IDisposable
{
    [Fact]
    public async void GivenInvalidMenuItem_WhenCreateMenuItem_ThenThrowValidationException()
    {
        //Arrange
        var context = ContextHelper.CreateInMemoryDBContext();
        var validator = new CreateMenuItemValidator();
        var logger = new NullLogger<CreateMenuItemHandler>();
        var handler = new CreateMenuItemHandler(context, logger, validator);
        var request = new CreateMenuItemRequest(string.Empty, 10);
        
        //Act and Assert
        
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await handler.Handle(request));
    }

    [Fact]
    public async void GivenValidMenuItem_WhenCreateMenuItem_ThenCreateMenuItem()
    {
        //Arrange
        var context = ContextHelper.CreateInMemoryDBContext();
        var validator = new CreateMenuItemValidator();
        var logger = new NullLogger<CreateMenuItemHandler>();
        var handler = new CreateMenuItemHandler(context, logger, validator);
        var request = new CreateMenuItemRequest("Test", 10);
        
        //Act
        
        var result = await handler.Handle(request);
        
        //Assert
        
        result.Should().NotBeNull();
        var menuItem = context.MenuItems.FirstOrDefault(m => m.Name == request.Name);
        menuItem.Should().NotBeNull();
        menuItem!.Name.Should().Be(request.Name);
        
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }
}