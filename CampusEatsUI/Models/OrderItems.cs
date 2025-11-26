namespace CampusEatsUI.Models;

public class OrderItems
{
    public Guid Id { get; set; }
    public Guid OrderId{ get; set;}
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string SpecialInstructions { get; set; } = string.Empty;
}