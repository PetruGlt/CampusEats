namespace CampusEats.Features.Users;

public record CreateUserRequest(
    string Username,
    string Email,
    string Password
    );