using Catalog.Contracts.Dtos;
using Shared.Contracts.CQRS;

namespace Catalog.Contracts.Features;

public record GetProductsByIdsQuery(List<Guid> ProductIds, Guid StoreId) : IQuery<GetProductsByIdsResult>;

public record GetProductsByIdsResult(List<ProductInfoDto> Products);
