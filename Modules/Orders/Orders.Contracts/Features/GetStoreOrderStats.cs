using Orders.Contracts.Dtos;
using Shared.Contracts.CQRS;

namespace Orders.Contracts.Features;

public record GetStoreOrderStatsQuery(Guid StoreId) : IQuery<GetStoreOrderStatsResult>;

public record GetStoreOrderStatsResult(StoreOrderStatsDto Stats);
