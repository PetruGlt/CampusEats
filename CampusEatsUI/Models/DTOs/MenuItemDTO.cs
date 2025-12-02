namespace CampusEatsUI.Models.DTOs;

public record CreateMenuItemRequest(string Name, decimal Price);
public record UpdateMenuItemRequest(Guid Id, string Name, decimal Price);