using System.Net.Http.Json;
using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.UserLoyalty;

public class UserLoyaltyService(HttpClient _http) : IUserLoyaltyService
{
    private const string BaseUrl = "http://localhost:5168/loyalty";
    public async Task<UserPoints> GetUserLoyaltyPointsAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<UserPoints>($"{BaseUrl}/{userId}");
    }
}