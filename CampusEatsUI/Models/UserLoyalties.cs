namespace CampusEatsUI.Models;

public record UserLoyalties
(
    Guid Id,
    Guid userId,
    int points,
    DateTime updatedAt, 
    DateTime createdAt
);