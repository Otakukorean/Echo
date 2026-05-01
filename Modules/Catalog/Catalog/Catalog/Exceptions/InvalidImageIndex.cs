using Shared.Exceptions;

namespace Catalog.Catalog.Exceptions;

public class InvalidImageIndex : BadRequestException
{
    public InvalidImageIndex(string message) : base(message) { }
}
