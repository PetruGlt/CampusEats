namespace CampusEats.Features.Loyalty;

public class UserLoyalty
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
