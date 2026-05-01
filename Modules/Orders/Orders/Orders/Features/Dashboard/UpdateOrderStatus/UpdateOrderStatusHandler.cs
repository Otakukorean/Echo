using Orders.Orders.Exceptions;

namespace Orders.Orders.Features.Dashboard.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, Guid StoreId, string Status) : ICommand<UpdateOrderStatusResponse>;

public record UpdateOrderStatusResponse(Guid OrderId, string Status);

public class UpdateOrderStatusHandler(OrdersDbContext dbContext)
    : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusResponse>
{
    public async Task<UpdateOrderStatusResponse> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .ForStore(request.StoreId)
            .SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken) ?? throw new OrderNotFound("Order not found");
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            throw new InvalidOrderStatus($"Invalid order status: {request.Status}");

        order.UpdateStatus(newStatus);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateOrderStatusResponse(order.Id, order.Status.ToString());
    }
}
