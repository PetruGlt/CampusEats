using CampusEats.Features.Menu;
using CampusEats.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Menus;

public class DeleteMenuItemHandlerTests : IDisposable
{
    [Fact]
    public async void GivenInvalidMenuItem_WhenDeleteMenuItem_ThenReturnNotFound()
    {
        //Arrange

        var context = ContextHelper.CreateInMemoryDBContext();
        var handler = new DeleteMenuItemHandler(context);
        var request = new DeleteMenuItemRequest(Guid.NewGuid());
        
        //Act
        var result = await handler.Handle(request);

        //Assert
        result.Should().NotBeNull();
        var notFoundResult = result as Microsoft.AspNetCore.Http.HttpResults.NotFound;
        notFoundResult.Should().NotBeNull();
    }

    [Fact]
    public async void GivenValidMenuItem_WhenDeleteMenuItem_ThenReturnsNoContent()
    {
        //Arrange
        var context = ContextHelper.CreateInMemoryDBContext();
        var menuItem = new MenuItem(Guid.NewGuid(), "Test", 10);
        
        context.Add(menuItem);
        context.SaveChanges();

        var handler = new DeleteMenuItemHandler(context);
        var request = new DeleteMenuItemRequest(menuItem.Id);
        
        //Act
        var result = await handler.Handle(request);
        
        //Assert
        var noContentResult = result as Microsoft.AspNetCore.Http.HttpResults.NoContent;
        noContentResult.Should().NotBeNull();
        
        var deletedItem = await context.MenuItems.FirstOrDefaultAsync(x => x.Id == menuItem.Id);
        deletedItem.Should().BeNull();
    }
    
    public void Dispose()
    {
        
    }
}