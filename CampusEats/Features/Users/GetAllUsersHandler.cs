using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Users;

public class GetAllUsersHandler(CampusEatsContext context)
{
    public async Task<IResult> Handle(GetAllUsersRequest request)
    {
        var users = await context.Users.ToListAsync();
        return Results.Ok(users);
    }
}