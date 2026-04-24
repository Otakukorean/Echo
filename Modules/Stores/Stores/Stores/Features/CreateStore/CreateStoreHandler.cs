using Stores.Stores.Exceptions;

namespace Stores.Stores.Features.CreateStore;

public record CreateStoreCommand(CreateStoreDto CreateStoreDto, Guid OwnerId) : ICommand<CreateStoreResponse>;

public record CreateStoreResponse(StoreDto Store);

public class CreateStoreHandler(StoresDbContext dbContext) : ICommandHandler<CreateStoreCommand, CreateStoreResponse>
{
    public async Task<CreateStoreResponse> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        // Check if user already owns a store
        var existingOwnerStore = await dbContext.Stores
            .AnyAsync(x => x.OwnerId == request.OwnerId, cancellationToken);
        if (existingOwnerStore)
        {
            throw new UserAlreadyHasStore("User already owns a store");
        }

        // Check for duplicate slug
        var slugExists = await dbContext.Stores
            .AnyAsync(x => x.Slug == request.CreateStoreDto.Slug, cancellationToken);
        if (slugExists)
        {
            throw new StoreAlreadyExist("A store with this slug already exists");
        }

        var dto = request.CreateStoreDto;
        var store = Store.Create(dto.Name, dto.Slug, dto.Description, dto.LogoUrl, dto.CoverUrl, request.OwnerId);
        dbContext.Stores.Add(store);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateStoreResponse(new StoreDto(
            store.Id, store.Name, store.Slug, store.Description,
            store.LogoUrl, store.CoverUrl, store.CreatedAt));
    }
}
