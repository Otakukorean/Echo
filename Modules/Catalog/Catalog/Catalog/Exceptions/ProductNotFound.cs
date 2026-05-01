using Shared.Exceptions;

namespace Catalog.Catalog.Exceptions;

public class ProductNotFound : NotFoundException
{
    public ProductNotFound(string message) : base(message) { }
}
