namespace CampusEats.Features.Loyalty;

public class UserLoyalty
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
