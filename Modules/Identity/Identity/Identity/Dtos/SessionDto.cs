namespace Identity.Identity.Dtos;

public record SessionDto(
    Guid Id,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    string CreatedByIp,
    string UserAgent,
    bool IsActive,
    DateTime? CreatedAt
);
