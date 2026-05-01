using Shared.Exceptions;

namespace Orders.Orders.Exceptions;

public class ProductUnavailable : BadRequestException
{
    public ProductUnavailable(string message) : base(message) { }
}
