namespace Stores.Stores.Services;

public class StoreOwnershipChecker : IStoreOwnershipChecker
{
    private readonly StoresDbContext _dbContext;

    public StoreOwnershipChecker(StoresDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VerifiedStore?> GetVerifiedStoreAsync(
        Guid storeId,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var store = await _dbContext.Stores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == storeId && s.OwnerId == ownerId, cancellationToken);

        if (store is null) return null;

        return new VerifiedStore(
            store.Id,
            store.Name,
            store.Slug,
            store.Description,
            store.OwnerId,
            store.LogoUrl,
            store.CoverUrl);
    }
}
