using Stores.Stores.Exceptions;

namespace Stores.Stores.Features.GetStoreBySlug;

public record GetStoreBySlugRequest(string Slug) : IQuery<GetStoreBySlugResponse>;

public record GetStoreBySlugResponse(StoreDto Store);

public class GetStoreBySlugHandler(StoresDbContext dbContext) : IQueryHandler<GetStoreBySlugRequest, GetStoreBySlugResponse>
{
    public async Task<GetStoreBySlugResponse> Handle(GetStoreBySlugRequest request, CancellationToken cancellationToken)
    {
        var store = await dbContext.Stores.SingleOrDefaultAsync(x => x.Slug == request.Slug, cancellationToken);
        if (store is null)
        {
            throw new StoreNotFound("Store not found");
        }
        return new GetStoreBySlugResponse(new StoreDto(
            store.Id, store.Name, store.Slug, store.Description,
            store.LogoUrl, store.CoverUrl, store.CreatedAt , store.OwnerId));
    }
}