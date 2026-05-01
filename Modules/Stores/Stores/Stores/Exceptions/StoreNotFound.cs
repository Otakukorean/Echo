using Shared.Exceptions;

namespace Stores.Stores.Exceptions;

public class StoreNotFound : NotFoundException
{
    public StoreNotFound(string message) : base(message)
    {
    }
}