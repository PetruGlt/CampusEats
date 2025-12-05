using CampusEats.Validators;
using CampusEats.Features.Menu;
using CampusEats.Persistence;
using CampusEats.Tests.Helpers;
using FluentValidation.TestHelper;
using FluentAssertions;
using Xunit;

namespace CampusEats.Tests.Validators;

public class UpdateMenuValidatorTests: IDisposable
{
    private UpdateMenuItemValidator _sut;
    private CampusEatsContext _context;

    public UpdateMenuValidatorTests()
    {
        _context = ContextHelper.CreateInMemoryDBContext();
        _sut = CreateSUT();
    }
    
    private UpdateMenuItemValidator CreateSUT() => new(_context);
    
    public void Dispose()
    {
        _sut = null;
        _context.Dispose();
    }
    
}