namespace Shared.Contracts.StoreContext;

public interface IStoreContext
{
    Guid UserId { get; }
    Guid? StoreId { get; }
    Task<VerifiedStore> GetVerifiedStoreAsync(CancellationToken cancellationToken = default);
}
