using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Users;

public class GetUserByIdHandler(CampusEatsContext context)
{
    public async Task<IResult> Handle(GetUserByIdRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (user == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(user);
    }
}