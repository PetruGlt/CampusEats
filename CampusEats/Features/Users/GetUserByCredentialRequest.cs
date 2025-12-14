namespace CampusEats.Features.Users;

public record GetUserByCredentialRequest(
    string Email,
    string PlainPassword
    );