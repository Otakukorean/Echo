using Shared.Exceptions;

namespace Identity.Identity.Exceptions;

public class UserAlreadyExist : BadRequestException
{
    public UserAlreadyExist(string message) : base(message)
    {
    }
}