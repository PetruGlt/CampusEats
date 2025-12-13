using CampusEats.Persistence;
using FluentValidation;
using BCrypt.Net;

namespace CampusEats.Features.Users;

public class CreateUserHandler(CampusEatsContext context, ILogger<CreateUserHandler> logger, IValidator<CreateUserRequest> validator)
{
    public async Task<IResult> Handler(CreateUserRequest request)
    {
        logger.LogInformation($"Creating user: {request.Username} with email: {request.Email}");

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            logger.LogError($"Validation Error! We couldn't create the User entity: user: {request.Username} with email: {request.Email}");
            return Results.BadRequest();
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(Guid.NewGuid(), request.Username, request.Email, hashedPassword);

        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        logger.LogInformation($"Created user: {request.Username} with email: {request.Email}");

        return Results.Created($"/users/{user.Id}", user);
    }
}