using Shared.Pagination;

namespace Orders.Orders.Features.Dashboard.GetOrders;

public record GetOrdersQuery(
    Guid StoreId,
    PaginationRequest Pagination,
    string? Status) : IQuery<GetOrdersResponse>;

public record GetOrdersResponse(PaginatedResult<OrderListDto> Orders);

public class GetOrdersHandler(OrdersDbContext dbContext)
    : IQueryHandler<GetOrdersQuery, GetOrdersResponse>
{
    public async Task<GetOrdersResponse> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var baseQuery = dbContext.Orders
            .AsNoTracking()
            .ForStore(query.StoreId);

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<OrderStatus>(query.Status, true, out var status))
        {
            baseQuery = baseQuery.Where(o => o.Status == status);
        }

        var totalCount = await baseQuery.LongCountAsync(cancellationToken);

        var pageIndex = query.Pagination.PageIndex;
        var pageSize = query.Pagination.PageSize;

        var orders = await baseQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListDto(
                o.Id,
                o.OrderNumber,
                o.CustomerName,
                o.Status.ToString(),
                o.Total,
                o.Items.Count,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetOrdersResponse(
            new PaginatedResult<OrderListDto>(pageIndex, pageSize, totalCount, orders));
    }
}
