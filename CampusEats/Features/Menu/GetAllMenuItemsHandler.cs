using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Menu;

public class GetAllMenuItemsHandler(CampusEatsContext context)
{
    public async Task<IResult> Handle(GetAllMenuItemsRequest request)
    {
        var menuItems = await context.MenuItems.ToListAsync();
        return Results.Ok(menuItems);
    }
}