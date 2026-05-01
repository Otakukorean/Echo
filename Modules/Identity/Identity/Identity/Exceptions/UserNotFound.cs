using Shared.Exceptions;

namespace Identity.Identity.Exceptions;

public class UserNotFound : BadRequestException
{
    public UserNotFound(string message) : base(message)
    {
    }
}
