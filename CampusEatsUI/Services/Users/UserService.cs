using System.Net.Http.Json;
using CampusEatsUI.Models.Requests.Users;
using Microsoft.AspNetCore.Components;

namespace CampusEatsUI.Services.Users;

public class UserService(HttpClient _http, NavigationManager _navigator) : IUserService
{
    public async Task<List<Models.Users>> GetAllUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<Models.Users>>($"/api/users").ContinueWith(t => t.Result ?? []);
    }
    
    public async Task<Models.Users> GetUserByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<Models.Users>($"/api/users/{id}")!;
    }

    public async Task UpdateUserAsync(Guid id, string username, string email, string plainPassword)
    {
        var request = new UpdateUserRequest(id, username, email, plainPassword);
        await _http.PutAsJsonAsync($"/api/users/{id}", request);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        await _http.DeleteAsync($"/api/users/{id}");
    }
}