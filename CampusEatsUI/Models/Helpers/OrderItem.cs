namespace CampusEatsUI.Models.Helpers;

public record OrderItem(
    Guid MenuItemId, 
    int Quantity, 
    string? SpecialInstructions
    );