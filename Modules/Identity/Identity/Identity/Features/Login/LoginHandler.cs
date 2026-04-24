namespace Identity.Identity.Features.Login;

public record LoginCommand(LoginDto LoginDto, string IpAddress, string UserAgent) : ICommand<LoginResult>;

public record LoginResponse(string AccessToken, DateTime ExpiresAt);

public record LoginResult(string AccessToken, DateTime ExpiresAt, string RefreshToken, DateTime RefreshExpiresAt);

public class LoginHandler(
    IdentityDbContext dbContext,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings,
    TimeProvider timeProvider) : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(x => x.Email == command.LoginDto.Email, cancellationToken);

        if (user is null)
        {
            throw new UserNotFound("User not found");
        }

        if (!user.IsActive)
        {
            throw new InvalidCredentials("Account is deactivated");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(command.LoginDto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new InvalidCredentials("Invalid credentials");
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var settings = jwtSettings.Value;
        var refreshExpiresAt = now.AddDays(settings.RefreshTokenExpirationInDays);
        var accessExpiresAt = now.AddMinutes(settings.AccessTokenExpirationInMinutes);

        // Remove existing session for the same user agent
        var existingSession = await dbContext.Sessions
            .Where(s => EF.Property<Guid>(s, "UserId") == user.Id && s.UserAgent == command.UserAgent)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingSession is not null)
        {
            dbContext.Sessions.Remove(existingSession);
        }

        var session = Session.Create(refreshToken, refreshExpiresAt, command.IpAddress, command.UserAgent);
        dbContext.Sessions.Add(session);
        dbContext.Entry(session).Property("UserId").CurrentValue = user.Id;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResult(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }
}
