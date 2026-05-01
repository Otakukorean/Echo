using Shared.Exceptions;

namespace Catalog.Catalog.Exceptions;

public class ProductImageNotFound : NotFoundException
{
    public ProductImageNotFound(string message) : base(message) { }
}
