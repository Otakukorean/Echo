using Shared.Exceptions;

namespace Identity.Identity.Exceptions;

public class UserNotFound : NotFoundException
{
    public UserNotFound(string message) : base(message)
    {
    }
}
