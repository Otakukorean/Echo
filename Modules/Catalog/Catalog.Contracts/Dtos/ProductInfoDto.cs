namespace Catalog.Contracts.Dtos;

public record ProductInfoDto(Guid Id, string Name, decimal Price, bool IsActive, int? AvailableQuantity);
