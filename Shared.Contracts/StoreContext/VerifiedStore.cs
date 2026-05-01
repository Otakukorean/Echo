namespace Shared.Contracts.StoreContext;

public record VerifiedStore(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    Guid OwnerId,
    string LogoUrl,
    string? CoverUrl);
