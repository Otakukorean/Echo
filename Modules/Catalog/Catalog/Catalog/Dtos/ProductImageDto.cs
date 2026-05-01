namespace Catalog.Catalog.Dtos;

public record ProductImageDto(Guid Id, string Url, bool IsPrimary, int Index);
