using System.Net.Http.Json;
using CampusEatsUI.Models.Requests.Users;
using Microsoft.AspNetCore.Components;

namespace CampusEatsUI.Services.Users;

public class UserService(HttpClient _http, NavigationManager _navigator) : IUserService
{
    private const string BaseUrl = "https://localhost:5168/users";

    public async Task<List<Models.Users>> GetAllUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<Models.Users>>($"{BaseUrl}").ContinueWith(t => t.Result ?? []);
    }
    
    public async Task<Models.Users> GetUserByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<Models.Users>($"{BaseUrl}/{id}")!;
    }

    public async Task UpdateUserAsync(Guid id, string username, string email, string plainPassword)
    {
        var request = new UpdateUserRequest(id, username, email, plainPassword);
        await _http.PutAsJsonAsync($"{BaseUrl}/{id}", request);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        await _http.DeleteAsync($"{BaseUrl}/{id}");
    }
}