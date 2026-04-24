using Shared.Exceptions;

namespace Stores.Stores.Exceptions;

public class UserAlreadyHasStore : BadRequestException
{
    public UserAlreadyHasStore(string message) : base(message)
    {
    }
}
