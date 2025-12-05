using CampusEats.Features.Menu;
using CampusEats.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CampusEats.Tests.Validators;

public class CreateMenuItemValidatorTests : IDisposable
{
    private CreateMenuItemValidator _sut;

    public CreateMenuItemValidatorTests()
    {
        _sut = CreateSUT();
    }

    [Fact]
    public void GivenEmptyName_WhenValidating_ThenShouldReturnError()
    {
        //Arrange
        var MenuModel = new CreateMenuItemRequest(string.Empty, 10);
        //Act
        var result = _sut.TestValidate(MenuModel);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void GivenShortName_WhenValidating_ThenShouldReturnError()
    {
        //Arrange
        var MenuModel = new CreateMenuItemRequest('a'.ToString(), 10);
        //Act
        var result = _sut.TestValidate(MenuModel);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void GivenNegativePrice_WhenValidating_ThenShouldReturnError()
    {
        //Arrange
        var MenuModel = new CreateMenuItemRequest("Test", -10);
        //Act
        var result = _sut.TestValidate(MenuModel);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void GivenValidParameters_WhenValidating_ThenShouldReturnTrue()
    {
        //Arrange
        var MenuModel = new CreateMenuItemRequest("Test", 10);
        //Act
        var result = _sut.TestValidate(MenuModel);
        //Assert
        result.IsValid.Should().BeTrue();
    }
    
    private CreateMenuItemValidator CreateSUT() => new();

    public void Dispose()
    {
        _sut = null;
    }
}