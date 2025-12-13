using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Users;

public class GetUserByCredentialHandler(CampusEatsContext context)
{
    public async Task<IResult> Handle(GetUserByCredentialRequest request)
    {
        var existingUser =
            await context.Users.FirstOrDefaultAsync(x => x.Username == request.Username && x.Email == request.Email);

        if (existingUser == null)
        {
            return Results.NotFound();
        }
        
        // Check the hash

        bool result = BCrypt.Net.BCrypt.Verify(request.PlainPassword, existingUser.HashedPassword);

        if (!result)
        {
            return Results.NotFound();
        }

        return Results.Ok(existingUser);
    }
}