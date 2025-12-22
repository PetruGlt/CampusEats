using CampusEatsUI.Models;

namespace CampusEatsUI.Services.Menu;

public interface IMenuService
{
    public void CreateMenuItemAsync(string name, decimal price);
    public void UpdateMenuItemAsync(Guid id, string name, decimal price);
    public void DeleteMenuItemAsync(Guid id);
    public Task<List<MenuItem>> GetAllMenuItemsAsync();
    public Task<MenuItem> GetMenuItemByIdAsync(Guid id);
}

