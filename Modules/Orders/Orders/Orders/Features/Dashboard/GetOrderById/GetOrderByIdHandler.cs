using Orders.Orders.Exceptions;

namespace Orders.Orders.Features.Dashboard.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId, Guid StoreId) : IQuery<GetOrderByIdResponse>;

public record GetOrderByIdResponse(OrderDto Order);

public class GetOrderByIdHandler(OrdersDbContext dbContext)
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResponse>
{
    public async Task<GetOrderByIdResponse> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ForStore(query.StoreId)
            .SingleOrDefaultAsync(o => o.Id == query.OrderId, cancellationToken);

        if (order is null)
            throw new OrderNotFound("Order not found");

        return new GetOrderByIdResponse(new OrderDto(
            order.Id, order.StoreId, order.OrderNumber,
            order.CustomerName, order.CustomerEmail, order.CustomerPhone,
            order.ShippingAddress, order.Status.ToString(),
            order.Subtotal, order.Total, order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductId, i.ProductName,
                i.UnitPrice, i.Quantity, i.LineTotal)).ToList()));
    }
}
