using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.Helpers;

public class CartState
{
    public Guid UserId { get; set; }
    public List<OrderItem> CartItems { get; set; } = new();
}