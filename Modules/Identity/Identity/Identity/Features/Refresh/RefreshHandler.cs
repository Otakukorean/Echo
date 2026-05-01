using Stores.Contracts.Features;

namespace Identity.Identity.Features.Refresh;

public record RefreshCommand(string RefreshToken, string IpAddress, string UserAgent) : ICommand<RefreshResult>;

public record RefreshResponse(string AccessToken, DateTime ExpiresAt);

public record RefreshResult(string AccessToken, DateTime ExpiresAt, string RefreshToken, DateTime RefreshExpiresAt);

public class RefreshHandler(
    IdentityDbContext dbContext,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings,
    TimeProvider timeProvider,
    ISender sender) : ICommandHandler<RefreshCommand, RefreshResult>
{
    public async Task<RefreshResult> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.Sessions)
            .SingleOrDefaultAsync(
                u => u.Sessions.Any(s => s.RefreshToken == command.RefreshToken),
                cancellationToken);

        if (user is null)
        {
            throw new InvalidSession("Invalid refresh token");
        }

        var session = user.Sessions.Single(s => s.RefreshToken == command.RefreshToken);

        if (!session.IsActive)
        {
            throw new InvalidSession("Session is no longer active");
        }

        session.Revoke();

        // Look up the user's store to include store_id in the new token
        var storeResult = await sender.Send(new GetStoreByOwnerIdQuery(user.Id), cancellationToken);
        var storeId = storeResult.Store?.StoreId;

        var accessToken = tokenService.GenerateAccessToken(user, storeId);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var settings = jwtSettings.Value;
        var refreshExpiresAt = now.AddDays(settings.RefreshTokenExpirationInDays);
        var accessExpiresAt = now.AddMinutes(settings.AccessTokenExpirationInMinutes);

        var newSession = Session.Create(newRefreshToken, refreshExpiresAt, command.IpAddress, command.UserAgent);
        dbContext.Sessions.Add(newSession);
        dbContext.Entry(newSession).Property("UserId").CurrentValue = user.Id;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshResult(accessToken, accessExpiresAt, newRefreshToken, refreshExpiresAt);
    }
}
