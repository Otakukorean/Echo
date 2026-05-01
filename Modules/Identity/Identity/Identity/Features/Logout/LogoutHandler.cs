using Identity.Identity.Exceptions;

namespace Identity.Identity.Features.Logout;

public record LogoutCommand(string RefreshToken) : ICommand<LogoutResponse>;

public record LogoutResponse(bool IsSuccess);

public class LogoutHandler(IdentityDbContext dbContext)
    : ICommandHandler<LogoutCommand, LogoutResponse>
{
    public async Task<LogoutResponse> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var session = await dbContext.Sessions
            .SingleOrDefaultAsync(s => s.RefreshToken == command.RefreshToken, cancellationToken);

        if (session is null)
            throw new InvalidSession("Invalid session");

        if (session.IsActive)
            session.Revoke();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LogoutResponse(true);
    }
}
