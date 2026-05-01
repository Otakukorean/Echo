using Shared.Contracts.CQRS;
using Stores.Contracts.Dtos;

namespace Stores.Contracts.Features;

public record GetStoreBySlugQuery(string Slug) : IQuery<GetStoreBySlugResult>;

public record GetStoreBySlugResult(StoreOwnerDto? Store);
