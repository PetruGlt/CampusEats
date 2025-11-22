namespace CampusEats.Exceptions;

public class MenuItemNotFoundException : BaseException
{
    protected internal MenuItemNotFoundException(Guid menuItemId) : base($"Menu Item with id {menuItemId} not found.", 404, "MENU_ITEM_NOT_FOUND")
    {
        
    }
}