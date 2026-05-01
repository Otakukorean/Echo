using Catalog.Contracts.Features;
using Orders.Orders.Exceptions;

namespace Orders.Orders.Features.Public.CreateOrder;

public record CreateOrderCommand(CreateOrderDto Dto, Guid StoreId) : ICommand<CreateOrderResponse>;

public record CreateOrderResponse(OrderDto Order);

public class CreateOrderHandler(OrdersDbContext dbContext, ISender sender)
    : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (dto.Items.Count == 0)
            throw new ProductUnavailable("Order must contain at least one item");

        // Get product info from Catalog module
        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var productsResult = await sender.Send(new GetProductsByIdsQuery(productIds, request.StoreId), cancellationToken);
        var productsMap = productsResult.Products.ToDictionary(p => p.Id);

        // Validate all products exist and are active
        foreach (var item in dto.Items)
        {
            if (!productsMap.TryGetValue(item.ProductId, out var product))
                throw new ProductUnavailable($"Product {item.ProductId} not found");

            if (!product.IsActive)
                throw new ProductUnavailable($"Product '{product.Name}' is not available");
        }

        // Generate order number
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var order = Order.Create(
            request.StoreId,
            orderNumber,
            dto.CustomerName,
            dto.CustomerEmail,
            dto.CustomerPhone,
            dto.ShippingAddress);

        foreach (var item in dto.Items)
        {
            var product = productsMap[item.ProductId];
            order.AddItem(product.Id, product.Name, product.Price, item.Quantity);
        }

        dbContext.Orders.Add(order);

        // Raise event after all items added so totals are calculated
        order.RaiseOrderCreatedEvent();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResponse(new OrderDto(
            order.Id, order.StoreId, order.OrderNumber,
            order.CustomerName, order.CustomerEmail, order.CustomerPhone,
            order.ShippingAddress, order.Status.ToString(),
            order.Subtotal, order.Total, order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductId, i.ProductName,
                i.UnitPrice, i.Quantity, i.LineTotal)).ToList()));
    }
}
