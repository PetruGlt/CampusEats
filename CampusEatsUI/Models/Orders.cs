namespace CampusEatsUI.Models;

public record Orders (
    Guid Id,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Notes
);