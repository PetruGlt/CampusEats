namespace CampusEats.Features.Orders;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Notes { get; set; }
}

public enum OrderStatus
{
    Pending,
    Preparing,
    Ready,
    Completed,
    Cancelled
}
