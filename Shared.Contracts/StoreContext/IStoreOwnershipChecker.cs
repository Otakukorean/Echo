namespace Shared.Contracts.StoreContext;

public interface IStoreOwnershipChecker
{
    Task<VerifiedStore?> GetVerifiedStoreAsync(Guid storeId, Guid ownerId, CancellationToken cancellationToken = default);
}
