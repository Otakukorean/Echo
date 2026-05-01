using Shared.Exceptions;

namespace Catalog.Catalog.Exceptions;

public class CategoryNotFound : NotFoundException
{
    public CategoryNotFound(string message) : base(message) { }
}
