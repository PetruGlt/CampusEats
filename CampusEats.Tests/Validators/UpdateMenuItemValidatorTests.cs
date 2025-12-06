using CampusEats.Features.Menu;
using CampusEats.Persistence;
using CampusEats.Validators;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Validators;

public class UpdateMenuItemValidatorTests
{
    private CampusEatsContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CampusEatsContext(options);
    }

    [Fact]
    public async Task GivenEmptyName_WhenValidating_ThenHaveError()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetInMemoryContext(dbName);
        var validator = new UpdateMenuItemValidator(context);
        
        // Constructorul record-ului: (Id, Name, Price)
        var request = new UpdateMenuItemRequest(Guid.NewGuid(), "", 10);

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task GivenPriceZeroOrNegative_WhenValidating_ThenHaveError()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetInMemoryContext(dbName);
        var validator = new UpdateMenuItemValidator(context);

        var requestZero = new UpdateMenuItemRequest(Guid.NewGuid(), "Burger", 0);
        var requestNegative = new UpdateMenuItemRequest(Guid.NewGuid(), "Burger", -5);

        // Act & Assert
        var resultZero = await validator.TestValidateAsync(requestZero);
        resultZero.ShouldHaveValidationErrorFor(x => x.Price);

        var resultNegative = await validator.TestValidateAsync(requestNegative);
        resultNegative.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public async Task GivenDuplicateNameForDifferentId_WhenValidating_ThenHaveError()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var existingId = Guid.NewGuid();
        var updateId = Guid.NewGuid(); // ID-ul itemului pe care vrem să-l modificăm

        using (var context = GetInMemoryContext(dbName))
        {
            // Adaugam un item deja existent cu numele "Pizza"
            // Folosim constructorul record-ului MenuItem(Id, Name, Price)
            context.MenuItems.Add(new MenuItem(existingId, "Pizza", 10));
            
            // Adaugam itemul pe care vrem sa il modificam (initial are alt nume)
            context.MenuItems.Add(new MenuItem(updateId, "Salad", 5));
            
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var validator = new UpdateMenuItemValidator(context);
            
            // Încercam sa redenumim "Salad" in "Pizza" (nume deja luat de existingId)
            var request = new UpdateMenuItemRequest(updateId, "Pizza", 15);

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            // Eroarea este pe nivelul obiectului, deoarece regula foloseste RuleFor(x => x)
            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("Another item with the same name already exists.");
        }
    }

    [Fact]
    public async Task GivenSameNameForSameId_WhenValidating_ThenNotHaveError()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var myId = Guid.NewGuid();

        using (var context = GetInMemoryContext(dbName))
        {
            // Avem "Pizza" in baza de date
            context.MenuItems.Add(new MenuItem(myId, "Pizza", 10));
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryContext(dbName))
        {
            var validator = new UpdateMenuItemValidator(context);
            
            // Facem update la ACELASI item, pastrand numele "Pizza" (ex: schimbam doar pretul)
            // Validatorul trebuie sa permita acest lucru (m.Id != req.Id va fi false)
            var request = new UpdateMenuItemRequest(myId, "Pizza", 20);

            // Act
            var result = await validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
    
    [Fact]
    public async Task GivenUniqueNameAndValidData_WhenValidating_ThenNotHaveError()
    {
         // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetInMemoryContext(dbName);
        var validator = new UpdateMenuItemValidator(context);
        
        var request = new UpdateMenuItemRequest(Guid.NewGuid(), "Unique Burger", 15);

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}