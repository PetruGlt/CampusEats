using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.UserLoyalty;

public class UserLoyaltyService(HttpClient _http) : IUserLoyaltyService
{
    public Task<UserPoints> GetUserLoyaltyPointsAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}