using Orders.Contracts.Dtos;
using Orders.Contracts.Features;

namespace Orders.Orders.Features.Internal.GetStoreOrderStats;

public class GetStoreOrderStatsHandler(OrdersDbContext dbContext)
    : IQueryHandler<GetStoreOrderStatsQuery, GetStoreOrderStatsResult>
{
    public async Task<GetStoreOrderStatsResult> Handle(GetStoreOrderStatsQuery query, CancellationToken cancellationToken)
    {
        var storeOrders = dbContext.Orders
            .AsNoTracking()
            .ForStore(query.StoreId);

        var totalRevenue = await storeOrders
            .Where(o => o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.Total, cancellationToken);

        var pending = await storeOrders.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var processing = await storeOrders.CountAsync(o => o.Status == OrderStatus.Processing, cancellationToken);
        var delivered = await storeOrders.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);

        return new GetStoreOrderStatsResult(
            new StoreOrderStatsDto(totalRevenue, new OrdersOverviewDto(pending, processing, delivered)));
    }
}
