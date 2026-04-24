namespace Stores.Stores.Dtos;

public record UpdateStoreDto(
    string Name,
    string Slug,
    string Description,
    string LogoUrl,
    string? CoverUrl
    );