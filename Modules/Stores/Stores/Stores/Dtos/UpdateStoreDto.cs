namespace Stores.Stores.Dtos;

public record UpdateStoreDto(
    string Name,
    string Slug,
    string Description,
    Stream? LogoStream,
    string? LogoFileName,
    string? LogoContentType,
    Stream? CoverStream,
    string? CoverFileName,
    string? CoverContentType);
