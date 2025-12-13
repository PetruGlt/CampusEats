namespace CampusEats.Features.Users;

public record UpdateUserRequest(
    Guid Id,
    string Username,
    string Email,
    string PlainPassword
);