namespace CampusEats.Features.Users;

public record RegisterRequest(
    string Username,
    string Email,
    string Password
    );