using CampusEats.Features.Kitchen;
using CampusEats.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CampusEats.Tests.Validators;

public class UpdateOrderStatusRequestBodyValidatorTests
{
    private readonly UpdateOrderStatusRequestBodyValidator _validator;

    public UpdateOrderStatusRequestBodyValidatorTests()
    {
        _validator = new UpdateOrderStatusRequestBodyValidator();
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void GivenInvalidStatus_WhenValidating_ThenHaveError(string? status)
    {
        // Arrange
        var request = new UpdateOrderStatusRequestBody(status!);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void GivenValidStatus_WhenValidating_ThenNotHaveError()
    {
        // Arrange
        var request = new UpdateOrderStatusRequestBody("Completed");

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }
}