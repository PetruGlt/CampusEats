namespace CampusEatsUI.Models.Helpers;

public record UserPoints(
    Guid UserId,
    int Points,
    DateTime LastUpdated);