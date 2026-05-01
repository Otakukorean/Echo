using Shared.Exceptions;

namespace Catalog.Catalog.Exceptions;

public class ProductSlugAlreadyExists : BadRequestException
{
    public ProductSlugAlreadyExists(string message) : base(message) { }
}
