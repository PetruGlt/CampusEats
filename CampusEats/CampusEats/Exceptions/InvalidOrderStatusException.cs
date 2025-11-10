using CampusEats.Exceptions;

namespace CampusEats.Exceptions;

public class InvalidOrderStatusException : BaseException
{
    public InvalidOrderStatusException(string message) 
        : base(message, 400, "INVALID_ORDER_STATUS")
    {
    }
}
