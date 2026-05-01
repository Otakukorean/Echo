using Shared.Contracts.FileStorage;
using Stores.Stores.Exceptions;

namespace Stores.Stores.Features.Dashboard.CreateStore;

public record CreateStoreCommand(CreateStoreDto CreateStoreDto, Guid OwnerId) : ICommand<CreateStoreResponse>;

public record CreateStoreResponse(StoreDto Store);

public class CreateStoreHandler(StoresDbContext dbContext, IFileStorageService fileStorage)
    : ICommandHandler<CreateStoreCommand, CreateStoreResponse>
{
    private const string ContainerName = "store-assets";

    public async Task<CreateStoreResponse> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        // Check if user already owns a store
        var existingOwnerStore = await dbContext.Stores
            .AnyAsync(x => x.OwnerId == request.OwnerId, cancellationToken);
        if (existingOwnerStore)
            throw new UserAlreadyHasStore("User already owns a store");

        // Check for duplicate slug
        var slugExists = await dbContext.Stores
            .AnyAsync(x => x.Slug == request.CreateStoreDto.Slug, cancellationToken);
        if (slugExists)
            throw new StoreAlreadyExist("A store with this slug already exists");

        var dto = request.CreateStoreDto;

        // Upload logo
        var logoResult = await fileStorage.UploadAsync(
            dto.LogoStream,
            dto.LogoFileName,
            dto.LogoContentType,
            ContainerName,
            $"{request.OwnerId}/logos",
            cancellationToken);

        // Upload cover (optional)
        string? coverUrl = null;
        if (dto.CoverStream is not null && dto.CoverFileName is not null && dto.CoverContentType is not null)
        {
            var coverResult = await fileStorage.UploadAsync(
                dto.CoverStream,
                dto.CoverFileName,
                dto.CoverContentType,
                ContainerName,
                $"{request.OwnerId}/covers",
                cancellationToken);
            coverUrl = coverResult.Url;
        }

        var store = Store.Create(dto.Name, dto.Slug, dto.Description, logoResult.Url, coverUrl, request.OwnerId);
        dbContext.Stores.Add(store);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateStoreResponse(new StoreDto(
            store.Id, store.Name, store.Slug, store.Description,
            store.LogoUrl, store.CoverUrl, store.CreatedAt, store.OwnerId));
    }
}
