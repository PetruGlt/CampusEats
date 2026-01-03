using CampusEats.Persistence;
using CampusEats.Service.Auth;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Users;

public class LoginHandler(CampusEatsContext context, ITokenService tokenService)
{
    public async Task<IResult> Handle(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PlainPassword))
        {
            return Results.BadRequest("Email and password are required.");
        }

        var existingUser =
            await context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        
        if (existingUser == null)
        {
            return Results.NotFound();
        }
        
        // Check the hash

        if (string.IsNullOrEmpty(existingUser.HashedPassword))
        {
            return Results.NotFound();
        }

        bool result = BCrypt.Net.BCrypt.Verify(request.PlainPassword, existingUser.HashedPassword);

        if (!result)
        {
            return Results.NotFound();
        }

        var token = tokenService.GenerateToken(existingUser.Email, existingUser.Username, existingUser.Id);
        
        return Results.Ok(new AuthResponse(token));
    }
}