namespace Identity.Identity.Features.Me;

public record MeQuery(Guid UserId) : IQuery<MeResponse>;

public record MeResponse(UserMeDto User);

public class MeHandler(IdentityDbContext dbContext) : IQueryHandler<MeQuery, MeResponse>
{
    public async Task<MeResponse> Handle(MeQuery query, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.Sessions)
            .SingleOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user is null)
        {
            throw new UserNotFound("User not found");
        }

        var sessions = user.Sessions
            .Select(s => new SessionDto(
                s.Id,
                s.ExpiresAt,
                s.RevokedAt,
                s.CreatedByIp,
                s.UserAgent,
                s.IsActive,
                s.CreatedAt
            ))
            .OrderByDescending(s => s.CreatedAt)
            .ToList();

        var dto = new UserMeDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsActive,
            user.EmailConfirmed,
            user.Role.ToString(),
            sessions
        );

        return new MeResponse(dto);
    }
}
