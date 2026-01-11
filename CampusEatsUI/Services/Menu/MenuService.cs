using System.Net.Http.Json;
using CampusEatsUI.Models;
using CampusEatsUI.Models.Requests;

namespace CampusEatsUI.Services.Menu;

public class MenuService(HttpClient _http) : IMenuService
{
    public async Task CreateMenuItemAsync(string name, decimal price)
    {
        var request = new CreateMenuItemRequest(name, price);
        await _http.PostAsJsonAsync("/api/menu", request);
    }

    public async Task UpdateMenuItemAsync(Guid id, string name, decimal price)
    {
        var request = new UpdateMenuItemRequest(id, name, price);
        await _http.PutAsJsonAsync($"/api/menu/{id}", request);
    }

    public async Task DeleteMenuItemAsync(Guid id)
    {
        await _http.DeleteAsync($"/api/menu/{id}");
    }

    public async Task<List<MenuItem>> GetAllMenuItemsAsync()
    {
        var items = await _http.GetFromJsonAsync<List<MenuItem>>("/api/menu");
        return items ?? null;
    }

    public Task<MenuItem> GetMenuItemByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}