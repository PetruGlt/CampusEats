namespace CampusEatsUI.Models;

public record Users(
    Guid Id,
    string Username,
    string Email,
    string HashedPassword
    );