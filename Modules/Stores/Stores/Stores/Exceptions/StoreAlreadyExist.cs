using Shared.Exceptions;

namespace Stores.Stores.Exceptions;

public class StoreAlreadyExist : BadRequestException
{
    public StoreAlreadyExist(string message) : base(message)
    {
    }
}
