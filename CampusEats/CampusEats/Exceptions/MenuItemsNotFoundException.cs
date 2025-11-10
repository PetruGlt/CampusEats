namespace CampusEats.Exceptions;

public class MenuItemsNotFoundException : BaseException
{
    public MenuItemsNotFoundException(List<Guid> missingIds) 
        : base($"Menu items not found: {string.Join(", ", missingIds)}", 404, "MENU_ITEMS_NOT_FOUND")
    {
    }
}
