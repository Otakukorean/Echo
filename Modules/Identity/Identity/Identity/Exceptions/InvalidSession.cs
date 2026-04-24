using Shared.Exceptions;

namespace Identity.Identity.Exceptions;

public class InvalidSession : UnauthorizedException
{
    public InvalidSession(string message) : base(message)
    {
    }
}
