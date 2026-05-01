namespace Orders.Orders.Dtos;

public record CreateOrderDto(
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string ShippingAddress,
    List<CreateOrderItemDto> Items);

public record CreateOrderItemDto(Guid ProductId, int Quantity);
