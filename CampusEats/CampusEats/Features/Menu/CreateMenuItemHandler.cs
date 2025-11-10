using CampusEats.Persistence;
using FluentValidation;

namespace CampusEats.Features.Menu;

public class CreateMenuItemHandler(CampusEatsContext context, ILogger<CreateMenuItemHandler> logger, IValidator<CreateMenuItemRequest> validator)
{
    public async Task<IResult> Handle(CreateMenuItemRequest request)
    {
        logger.LogInformation("Creating a new menu item: {Name}, Price: {Price}", request.Name, request.Price);
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        var menuItem = new MenuItem(Guid.NewGuid(), request.Name, request.Price);
        context.MenuItems.Add(menuItem);

        await context.SaveChangesAsync();
        logger.LogInformation("Menu item created successfully with ID: {menuItemId}", menuItem.Id);

        return Results.Created($"/menu/{menuItem.Id}", menuItem);
    }
}