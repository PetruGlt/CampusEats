namespace CampusEatsUI.Models.DTOs;

public record CreateUserRequest(
    string Username,
    string Email,
    string Password
);

public record GetUserByCredentialRequest(
    string Email,
    string PlainPassword
);