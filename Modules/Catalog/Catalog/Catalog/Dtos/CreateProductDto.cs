namespace Catalog.Catalog.Dtos;

public record CreateProductDto(
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive,
    string? Sku,
    List<Guid> CategoryIds
    );