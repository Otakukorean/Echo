namespace Orders.Orders.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid StoreId,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    decimal Total,
    List<OrderItemInfo> Items) : IDomainEvent;

public record OrderItemInfo(string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
