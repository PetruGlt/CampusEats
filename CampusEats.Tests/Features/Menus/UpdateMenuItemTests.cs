using AutoMapper;
using CampusEats.Features.Menu;
using CampusEats.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace CampusEats.Tests.Features.Menus;

public class UpdateMenuItemHandlerTests : IDisposable
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<UpdateMenuItemRequest>> _validatorMock;

    public UpdateMenuItemHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<UpdateMenuItemRequest>>();
    }

    [Fact]
    public async Task GivenInvalidRequest_WhenUpdateMenuItem_ThenThrowValidationException()
    {
        // Arrange
        var context = ContextHelper.CreateInMemoryDBContext();
        var handler = new UpdateMenuItemHandler(context, _mapperMock.Object, _validatorMock.Object);
        
        var request = new UpdateMenuItemRequest(Guid.NewGuid(), "Bad Name", 0);

        // Setup Validator to fail
        var validationFailure = new List<ValidationFailure> { new("Name", "Error") };
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailure));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(async () => await handler.Handle(request));
    }

    [Fact]
    public async Task GivenValidRequest_WhenUpdateMenuItem_ThenUpdatesDatabase()
    {
        // Arrange
        var context = ContextHelper.CreateInMemoryDBContext();
        
       
        var existingId = Guid.NewGuid();
        
        var existingItem = new MenuItem(existingId, "Old Burger", 10m);
        
        context.MenuItems.Add(existingItem);
        await context.SaveChangesAsync();
        
        context.ChangeTracker.Clear(); 

        var request = new UpdateMenuItemRequest(existingId, "New Burger", 15m);
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

       
        var mappedMenuItem = new MenuItem(existingId, "New Burger", 15m);
        _mapperMock.Setup(m => m.Map<MenuItem>(request)).Returns(mappedMenuItem);

        var handler = new UpdateMenuItemHandler(context, _mapperMock.Object, _validatorMock.Object);

        // Act
        var result = await handler.Handle(request);

        // Assert
        result.Should().NotBeNull();
        
        // Fetch from DB to verify persistence
        var dbItem = await context.MenuItems.FindAsync(existingId);
        
        dbItem.Should().NotBeNull();
        dbItem!.Name.Should().Be("New Burger");
        dbItem.Price.Should().Be(15m);
    }

    public void Dispose()
    {
        // Clean up managed resources if necessary
    }
}