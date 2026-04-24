using Shared.Exceptions;

namespace Identity.Identity.Exceptions;

public class InvalidCredentials : BadRequestException
{
    public InvalidCredentials(string message) : base(message)
    {
    }
}
