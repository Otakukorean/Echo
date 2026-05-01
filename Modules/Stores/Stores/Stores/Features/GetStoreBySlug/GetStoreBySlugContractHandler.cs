using Stores.Contracts.Dtos;
using Stores.Contracts.Features;

namespace Stores.Stores.Features.GetStoreBySlug;

public class GetStoreBySlugContractHandler(StoresDbContext dbContext)
    : IQueryHandler<GetStoreBySlugQuery, GetStoreBySlugResult>
{
    public async Task<GetStoreBySlugResult> Handle(GetStoreBySlugQuery query, CancellationToken cancellationToken)
    {
        var store = await dbContext.Stores
            .AsNoTracking()
            .Where(s => s.Slug == query.Slug)
            .Select(s => new StoreOwnerDto(s.Id, s.OwnerId))
            .FirstOrDefaultAsync(cancellationToken);

        return new GetStoreBySlugResult(store);
    }
}
