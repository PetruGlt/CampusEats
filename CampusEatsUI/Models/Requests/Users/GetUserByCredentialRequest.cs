namespace CampusEatsUI.Models.Requests.Users;

public record GetUserByCredentialRequest(string Email, string PlainPassword);