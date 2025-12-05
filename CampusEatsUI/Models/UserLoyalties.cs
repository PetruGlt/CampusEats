namespace CampusEatsUI.Models;

public class UserLoyalties
{
    public Guid Id { get; set; }
    public Guid userId { get; set; } 
    public int points { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}