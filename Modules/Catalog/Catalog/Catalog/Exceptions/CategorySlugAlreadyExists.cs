using Shared.Exceptions;

namespace Catalog.Catalog.Exceptions;

public class CategorySlugAlreadyExists : BadRequestException
{
    public CategorySlugAlreadyExists(string message) : base(message) { }
}
