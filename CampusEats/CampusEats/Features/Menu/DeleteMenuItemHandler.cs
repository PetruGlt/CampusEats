using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Menu;

public class DeleteMenuItemHandler(CampusEatsContext context)
{
    public async Task<IResult> Handle(DeleteMenuItemRequest request)
    {
        var item = await context.MenuItems.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (item == null)
        {
            return Results.NotFound(); 
        }
        context.MenuItems.Remove(item);
        await context.SaveChangesAsync();
        return Results.Ok();
    }
    
}