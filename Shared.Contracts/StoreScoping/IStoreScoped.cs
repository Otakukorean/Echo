namespace Shared.Contracts.StoreScoping;

public interface IStoreScoped
{
    Guid StoreId { get; }
}
