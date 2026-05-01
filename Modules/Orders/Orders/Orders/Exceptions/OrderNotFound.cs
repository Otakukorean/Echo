using Shared.Exceptions;

namespace Orders.Orders.Exceptions;

public class OrderNotFound : NotFoundException
{
    public OrderNotFound(string message) : base(message) { }
}
