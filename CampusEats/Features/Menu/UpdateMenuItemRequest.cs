namespace CampusEats.Features.Menu;

public record UpdateMenuItemRequest(Guid Id, string Name, decimal Price);