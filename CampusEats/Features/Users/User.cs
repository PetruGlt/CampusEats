namespace CampusEats.Features.Users;

public record User(
    Guid Id,
    string Username,
    string Email,
    string HashedPassword
    );