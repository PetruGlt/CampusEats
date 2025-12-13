namespace CampusEats.Features.Users;

public record GetUserByCredentialRequest(
    string Username,
    string Email,
    string PlainPassword
    );