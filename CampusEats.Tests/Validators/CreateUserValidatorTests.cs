using CampusEats.Features.Users;
using CampusEats.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CampusEats.Tests.Validators;

public class CreateUserValidatorTests
{
    private CreateUserValidator _sut;

    public CreateUserValidatorTests()
    {
        _sut = CreateSut();
    }

    [Fact]
    public void Given_EmptyOrShortUsername_When_Validating_Then_ReturnError()
    {
        //Arrange
        var user = new RegisterRequest(string.Empty, "test@example.ro", "password123");
        
        //Act
        var result = _sut.TestValidate(user);
        
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
    [Fact]
    public void Given_EmptyOrInvalidEmail_When_Validating_Then_ReturnError()
    {
        //Arrange
        var user = new RegisterRequest("test_user", string.Empty, "password123");
        
        //Act
        var result = _sut.TestValidate(user);
        
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
    [Fact]
    public void Given_EmptyOrShortPassword_When_Validating_Then_ReturnError()
    {
        //Arrange
        var user = new RegisterRequest("test_user", "test@example.ro", string.Empty);
        
        //Act
        var result = _sut.TestValidate(user);
        
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
    [Fact]
    public void Given_CorectCredential_When_Validating_Then_ReturnSuccess()
    {
        //Arrange
        var user = new RegisterRequest("test_user", "test@example.ro", "password123");
        
        //Act
        var result = _sut.TestValidate(user);
        
        //Assert
        result.IsValid.Should().BeTrue();
    }

    private CreateUserValidator CreateSut() => new();
}