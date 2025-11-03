using System;
using System.Collections.Generic;

namespace CampusEats.Features.Orders
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Preparing, Ready, Completed
    }

    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public Guid MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}
