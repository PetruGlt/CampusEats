namespace CampusEatsUI.Models.Requests;

public record UpdateMenuItemRequest(
    Guid Id,
    string Name,
    decimal Price);