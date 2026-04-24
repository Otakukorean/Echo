namespace Stores.Stores.Dtos;

public record CreateStoreDto(
    string Name,
    string Slug,
    string Description,
    string LogoUrl,
    string? CoverUrl
    );