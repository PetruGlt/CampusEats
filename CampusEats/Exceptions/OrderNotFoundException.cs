using CampusEats.Exceptions;

namespace CampusEats.Exceptions;

public class OrderNotFoundException : BaseException
{
    public OrderNotFoundException(Guid orderId) 
        : base($"Order with ID {orderId} not found", 404, "ORDER_NOT_FOUND")
    {
    }
}
