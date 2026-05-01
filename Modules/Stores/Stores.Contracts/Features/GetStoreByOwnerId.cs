using Shared.Contracts.CQRS;
using Stores.Contracts.Dtos;

namespace Stores.Contracts.Features;

public record GetStoreByOwnerIdQuery(Guid OwnerId) : IQuery<GetStoreByOwnerIdResult>;

public record GetStoreByOwnerIdResult(StoreOwnerDto? Store);
