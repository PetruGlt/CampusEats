using CampusEats.Validators;

namespace CampusEats.Tests.Validators;

public class UpdateOrderStatusRequestBodyValidatorTests
{
    private UpdateOrderStatusRequestBodyValidator _sut;

    public UpdateOrderStatusRequestBodyValidatorTests()
    {
        _sut = CreateSUT();
    }
    
    private UpdateOrderStatusRequestBodyValidator CreateSUT() => new();

    public void Dispose()
    {
        _sut = null;
    }
}