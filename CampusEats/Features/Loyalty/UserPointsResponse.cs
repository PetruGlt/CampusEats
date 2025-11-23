namespace CampusEats.Features.Loyalty;

public class UserPointsResponse
{
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public DateTime? LastUpdated { get; set; }
}
