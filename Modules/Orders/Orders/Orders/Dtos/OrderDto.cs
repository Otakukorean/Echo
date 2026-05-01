namespace Orders.Orders.Dtos;

public record OrderDto(
    Guid Id,
    Guid StoreId,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string ShippingAddress,
    string Status,
    decimal Subtotal,
    decimal Total,
    DateTime? CreatedAt,
    List<OrderItemDto> Items);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record OrderListDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string Status,
    decimal Total,
    int ItemCount,
    DateTime? CreatedAt);
