namespace Catalog.Catalog.Dtos;

public record ProductDto(
    Guid Id,
    Guid StoreId,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive,
    string? Sku,
    List<CategoryDto> Categories,
    List<ImagesDto> Images,
    List<VariationDto> Variations
    );

public record CategoryDto(Guid Id, string Name, string Slug);

public record ImagesDto(Guid Id, string Url, bool IsPrimary, int Index);

public record VariationDto(Guid Id, string Value, decimal Price , bool Active , string? Color , string? Url , int Quantity);
