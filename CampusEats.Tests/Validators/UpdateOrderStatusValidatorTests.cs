using CampusEats.Validators;

namespace CampusEats.Tests.Validators;

public class UpdateOrderStatusValidatorTests
{
    private UpdateOrderStatusValidator _sut;

    public UpdateOrderStatusValidatorTests()
    {
        _sut = CreateSUT();
    }
    
    private UpdateOrderStatusValidator CreateSUT() => new();

    public void Dispose()
    {
        _sut = null;
    }
}