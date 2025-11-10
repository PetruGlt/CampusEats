using CampusEats.Exceptions;
using CampusEats.Persistence;

namespace CampusEats.Features.Menu;

public class GetByIdMenuItemHandler(CampusEatsContext dbContext)
{
    public async Task<IResult> Handle(GetByIdMenuItemRequest request)
    {
        var menuItem = await dbContext.MenuItems.FindAsync(request.Id);
        if (menuItem == null)
        {
            throw new MenuItemNotFoundException(request.Id);
        }
        return Results.Ok(menuItem);
    }
}