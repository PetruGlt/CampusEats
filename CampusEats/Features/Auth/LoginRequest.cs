namespace CampusEats.Features.Users;

public record LoginRequest(
    string Email,
    string PlainPassword
    );