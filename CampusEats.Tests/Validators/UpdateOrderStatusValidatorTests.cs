using CampusEats.Features.Kitchen; 
using CampusEats.Validators;
using FluentValidation.TestHelper;


namespace CampusEats.Tests.Validators;

public class UpdateOrderStatusValidatorTests
{
    private readonly UpdateOrderStatusValidator _validator;

    public UpdateOrderStatusValidatorTests()
    {
        _validator = new UpdateOrderStatusValidator();
    }

    [Fact]
    public void GivenEmptyId_WhenValidating_ThenHaveError()
    {
        // Arrange
        var request = new UpdateOrderStatusRequest(Guid.Empty, "Pending");

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void GivenEmptyStatus_WhenValidating_ThenHaveError()
    {
        // Arrange
        var request = new UpdateOrderStatusRequest(Guid.NewGuid(), string.Empty);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Status)
              .WithErrorMessage("Status is required");
    }
    
    [Theory]
    [InlineData("Pending")]
    [InlineData("Preparing")]
    [InlineData("Ready")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void GivenValidStatusString_WhenValidating_ThenNotHaveError(string validStatus)
    {
        // Arrange
        var request = new UpdateOrderStatusRequest(Guid.NewGuid(), validStatus);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }
}