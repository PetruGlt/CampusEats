using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Loyalty;

public class LoyaltyService
{
    private readonly CampusEatsContext _context;

    public LoyaltyService(CampusEatsContext context)
    {
        _context = context;
    }

    public async Task<int> AddPointsForOrder(Guid userId, decimal orderAmount)
    {
        // Award 10 points per dollar
        var pointsToAdd = (int)(orderAmount * 10);

        var userLoyalty = await _context.UserLoyalties
            .FirstOrDefaultAsync(ul => ul.UserId == userId);

        if (userLoyalty == null)
        {
            userLoyalty = new UserLoyalty
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Points = pointsToAdd,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UserLoyalties.Add(userLoyalty);
        }
        else
        {
            userLoyalty.Points += pointsToAdd;
            userLoyalty.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return pointsToAdd;
    }

    public async Task<UserLoyalty?> GetUserLoyalty(Guid userId)
    {
        return await _context.UserLoyalties
            .FirstOrDefaultAsync(ul => ul.UserId == userId);
    }
}

