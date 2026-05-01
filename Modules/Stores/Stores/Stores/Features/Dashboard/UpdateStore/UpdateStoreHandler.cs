using Stores.Stores.Exceptions;

namespace Stores.Stores.Features.Dashboard.UpdateStore;

public record UpdateStoreRequest(UpdateStoreDto UpdateStoreDto , Guid StoreId) : ICommand<UpdateStoreResponse>;

public record UpdateStoreResponse(StoreDto Store);

public class UpdateStoreHandler(StoresDbContext dbContext) : ICommandHandler<UpdateStoreRequest, UpdateStoreResponse>
{
    public async Task<UpdateStoreResponse> Handle(UpdateStoreRequest request, CancellationToken cancellationToken)
    {
        var store = await dbContext.Stores.SingleOrDefaultAsync(x => x.Id == request.StoreId, cancellationToken);
        if (store is null)
        {
            throw new StoreNotFound("Store not found");
        }
        MapToDto(store , request.UpdateStoreDto);
        dbContext.Stores.Update(store);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateStoreResponse(new StoreDto(store.Id, store.Name, store.Slug, store.Description, store.LogoUrl, store.CoverUrl, store.CreatedAt, store.OwnerId));
    }
    
    private static void MapToDto(Store store , UpdateStoreDto request)
    {
        store.Update(request.Name, request.Slug, request.Description, request.LogoUrl, request.CoverUrl);
    }
}