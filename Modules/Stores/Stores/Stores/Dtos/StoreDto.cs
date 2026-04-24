namespace Stores.Stores.Dtos;

public record StoreDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string LogoUrl,
    string? CoverUrl,
    DateTime? CreatedAt
    );