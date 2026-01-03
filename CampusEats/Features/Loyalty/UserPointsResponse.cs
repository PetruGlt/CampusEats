namespace CampusEats.Features.Loyalty;

public class UserPointsResponse
{
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public DateTime? LastUpdated { get; set; }
}
