using Shared.Exceptions;

namespace Orders.Orders.Exceptions;

public class InvalidOrderStatus : BadRequestException
{
    public InvalidOrderStatus(string message) : base(message) { }
}
