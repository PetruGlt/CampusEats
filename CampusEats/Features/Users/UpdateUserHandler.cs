using CampusEats.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Users;

public class UpdateUserHandler(CampusEatsContext context, IValidator<UpdateUserRequest> validator)
{
    public async Task<IResult> Handle(UpdateUserRequest request)
    {
        var validatorResult = await validator.ValidateAsync(request);
        if(!validatorResult.IsValid)
        {
            return Results.BadRequest(validatorResult.Errors);
        }

        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (user == null)
        {
            return Results.NotFound();
        }

        var updatedUsername = user.Username == null ? request.Username : user.Username;
        var updatedEmail = user.Email == null ? request.Email : user.Email;
        var updatedPassword = user.HashedPassword == null ? BCrypt.Net.BCrypt.HashPassword(request.PlainPassword) : user.HashedPassword;
        
        var updatedUser = new User(request.Id, updatedUsername, updatedEmail, updatedPassword);
        
        context.Users.Update(updatedUser);
        await context.SaveChangesAsync();
        
        return Results.NoContent();
    }
}