namespace CampusEatsUI.Models;

public record Orders (
    Guid Id,
    Guid UserId,
    List<OrderItems> Items,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Notes
);