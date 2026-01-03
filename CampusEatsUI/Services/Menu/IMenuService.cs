using CampusEatsUI.Models;

namespace CampusEatsUI.Services.Menu;

public interface IMenuService
{
    public Task CreateMenuItemAsync(string name, decimal price);
    public Task UpdateMenuItemAsync(Guid id, string name, decimal price);
    public Task DeleteMenuItemAsync(Guid id);
    public Task<List<MenuItem>> GetAllMenuItemsAsync();
    public Task<MenuItem> GetMenuItemByIdAsync(Guid id);
}

