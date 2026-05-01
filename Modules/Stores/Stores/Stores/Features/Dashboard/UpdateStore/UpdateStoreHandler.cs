using Shared.Contracts.FileStorage;
using Stores.Stores.Exceptions;

namespace Stores.Stores.Features.Dashboard.UpdateStore;

public record UpdateStoreRequest(UpdateStoreDto UpdateStoreDto, Guid StoreId) : ICommand<UpdateStoreResponse>;

public record UpdateStoreResponse(StoreDto Store);

public class UpdateStoreHandler(StoresDbContext dbContext, IFileStorageService fileStorage)
    : ICommandHandler<UpdateStoreRequest, UpdateStoreResponse>
{
    private const string ContainerName = "store-assets";

    public async Task<UpdateStoreResponse> Handle(UpdateStoreRequest request, CancellationToken cancellationToken)
    {
        var store = await dbContext.Stores.SingleOrDefaultAsync(x => x.Id == request.StoreId, cancellationToken);
        if (store is null)
            throw new StoreNotFound("Store not found");

        var dto = request.UpdateStoreDto;

        // Upload new logo if provided, delete old one
        var logoUrl = store.LogoUrl;
        if (dto.LogoStream is not null && dto.LogoFileName is not null && dto.LogoContentType is not null)
        {
            if (!string.IsNullOrWhiteSpace(store.LogoUrl))
                await fileStorage.DeleteAsync(store.LogoUrl, cancellationToken);

            var logoResult = await fileStorage.UploadAsync(
                dto.LogoStream, dto.LogoFileName, dto.LogoContentType,
                ContainerName, $"{store.OwnerId}/logos", cancellationToken);
            logoUrl = logoResult.Url;
        }

        // Upload new cover if provided, delete old one
        var coverUrl = store.CoverUrl;
        if (dto.CoverStream is not null && dto.CoverFileName is not null && dto.CoverContentType is not null)
        {
            if (!string.IsNullOrWhiteSpace(store.CoverUrl))
                await fileStorage.DeleteAsync(store.CoverUrl, cancellationToken);

            var coverResult = await fileStorage.UploadAsync(
                dto.CoverStream, dto.CoverFileName, dto.CoverContentType,
                ContainerName, $"{store.OwnerId}/covers", cancellationToken);
            coverUrl = coverResult.Url;
        }

        store.Update(dto.Name, dto.Slug, dto.Description, logoUrl, coverUrl);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateStoreResponse(new StoreDto(
            store.Id, store.Name, store.Slug, store.Description,
            store.LogoUrl, store.CoverUrl, store.CreatedAt, store.OwnerId));
    }
}
