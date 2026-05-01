namespace Catalog.Catalog.Dtos;

public record UpdateVariationDto(
    string Value,
    decimal Price,
    bool Active,
    string? Color,
    int Quantity,
    Stream? FileStream,
    string? FileName,
    string? ContentType);
