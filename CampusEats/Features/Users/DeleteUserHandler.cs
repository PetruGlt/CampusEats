using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Users;

public class DeleteUserHandler(CampusEatsContext context)
{
    public async Task<IResult> Handle(DeleteUserRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (user == null)
        {
            return Results.NotFound();
        }
        
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
}