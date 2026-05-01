using Shared.Exceptions;

namespace Catalog.Catalog.Exceptions;

public class VariationNotFound : NotFoundException
{
    public VariationNotFound(string message) : base(message) { }
}
