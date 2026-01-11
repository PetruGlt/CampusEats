using System.Net.Http.Json;
using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.UserLoyalty;

public class UserLoyaltyService(HttpClient _http) : IUserLoyaltyService
{
    public async Task<UserPoints> GetUserLoyaltyPointsAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<UserPoints>($"/api/loyalty/{userId}");
    }
}