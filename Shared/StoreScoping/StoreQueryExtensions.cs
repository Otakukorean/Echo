using Shared.Contracts.StoreScoping;

namespace Shared.StoreScoping;

public static class StoreQueryExtensions
{
    public static IQueryable<T> ForStore<T>(this IQueryable<T> query, Guid storeId)
        where T : class, IStoreScoped
        => query.Where(e => e.StoreId == storeId);
}
