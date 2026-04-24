namespace Identity.Identity.Models;

public class Session : Aggregate<Guid>
{
    public string RefreshToken { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string CreatedByIp { get; private set; }
    public string UserAgent { get; private set; }

    private Session() { }

    public static Session Create(string refreshToken, DateTime expiresAt, string createdByIp, string userAgent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdByIp);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

        return new Session
        {
            Id = Guid.NewGuid(),
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            UserAgent = userAgent,
        };
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}
