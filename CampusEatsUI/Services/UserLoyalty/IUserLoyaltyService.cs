using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.UserLoyalty;

public interface IUserLoyaltyService
{
    public Task<UserPoints> GetUserLoyaltyPointsAsync(Guid id);
}