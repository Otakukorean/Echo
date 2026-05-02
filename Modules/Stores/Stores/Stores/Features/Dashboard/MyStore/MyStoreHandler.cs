using Orders.Contracts.Dtos;
using Orders.Contracts.Features;
using Stores.Stores.Exceptions;

namespace Stores.Stores.Features.Dashboard.MyStore;

public record MyStoreQuery(Guid StoreId) : IQuery<MyStoreResponse>;

public record MyStoreResponse(StoreDto Store, StoreOrderStatsDto OrderStats);

public class MyStoreHandler(StoresDbContext dbContext, ISender sender)
    : IQueryHandler<MyStoreQuery, MyStoreResponse>
{
    public async Task<MyStoreResponse> Handle(MyStoreQuery query, CancellationToken cancellationToken)
    {
        var store = await dbContext.Stores
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == query.StoreId, cancellationToken);

        if (store is null)
            throw new StoreNotFound("Store not found");

        var statsResult = await sender.Send(new GetStoreOrderStatsQuery(store.Id), cancellationToken);

        return new MyStoreResponse(
            new StoreDto(store.Id, store.Name, store.Slug, store.Description,
                store.LogoUrl, store.CoverUrl, store.CreatedAt, store.OwnerId),
            statsResult.Stats);
    }
}
