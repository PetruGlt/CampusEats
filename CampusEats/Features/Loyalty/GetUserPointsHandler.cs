using CampusEats.Features.Loyalty;

namespace CampusEats.Features.Loyalty;

public class GetUserPointsHandler
{
    private readonly LoyaltyService _loyaltyService;

    public GetUserPointsHandler(LoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    public async Task<UserPointsResponse> Handle(GetUserPointsRequest request)
    {
        var userLoyalty = await _loyaltyService.GetUserLoyalty(request.UserId);
        
        return new UserPointsResponse
        {
            UserId = request.UserId,
            Points = userLoyalty?.Points ?? 0,
            LastUpdated = userLoyalty?.UpdatedAt
        };
    }
}
