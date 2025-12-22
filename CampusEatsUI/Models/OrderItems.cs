namespace CampusEatsUI.Models;

public record OrderItems
(
    Guid Id,
    Guid OrderId,
    Guid MenuItemId,
    string MenuItemName,
    decimal Price,
    int Quantity,
    string? SpecialInstructions
);