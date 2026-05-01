using Stores.Contracts.Dtos;
using Stores.Contracts.Features;

namespace Stores.Stores.Features.GetStoreByOwnerId;

public class GetStoreByOwnerIdHandler(StoresDbContext dbContext)
    : IQueryHandler<GetStoreByOwnerIdQuery, GetStoreByOwnerIdResult>
{
    public async Task<GetStoreByOwnerIdResult> Handle(GetStoreByOwnerIdQuery query, CancellationToken cancellationToken)
    {
        var store = await dbContext.Stores
            .AsNoTracking()
            .Where(s => s.OwnerId == query.OwnerId)
            .Select(s => new StoreOwnerDto(s.Id, s.OwnerId))
            .FirstOrDefaultAsync(cancellationToken);

        return new GetStoreByOwnerIdResult(store);
    }
}
