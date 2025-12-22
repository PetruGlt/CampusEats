namespace CampusEatsUI.Models.Helpers;

public record UserPoints(
    Guid Id,
    int Points,
    DateTime LastUpdate);