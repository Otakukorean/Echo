namespace Shared.Contracts.CQRS;

public interface IRequireStoreOwnership
{
    Guid StoreId { get; }
}