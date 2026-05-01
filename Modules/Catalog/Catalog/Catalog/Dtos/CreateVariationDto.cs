namespace Catalog.Catalog.Dtos;

public record CreateVariationDto(
    string Value,
    decimal Price,
    bool Active,
    string? Color,
    int Quantity,
    Stream? FileStream,
    string? FileName,
    string? ContentType);
