using Orders.Orders.Events;
using Shared.Contracts.StoreScoping;

namespace Orders.Orders.Models;

public class Order : Aggregate<Guid>, IStoreScoped
{
    public Guid StoreId { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public string CustomerName { get; private set; } = null!;
    public string CustomerEmail { get; private set; } = null!;
    public string CustomerPhone { get; private set; } = null!;
    public string ShippingAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Total { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(
        Guid storeId, string orderNumber, string customerName,
        string customerEmail, string customerPhone, string shippingAddress)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            OrderNumber = orderNumber,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Pending,
            Subtotal = 0,
            Total = 0
        };
    }

    public OrderItem AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var item = OrderItem.Create(Id, productId, productName, unitPrice, quantity);
        _items.Add(item);
        RecalculateTotals();
        return item;
    }

    public void RaiseOrderCreatedEvent()
    {
        AddDomainEvent(new OrderCreatedEvent(
            Id, StoreId, OrderNumber, CustomerName, CustomerEmail, Total,
            _items.Select(i => new OrderItemInfo(i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal)).ToList()));
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
    }

    private void RecalculateTotals()
    {
        Subtotal = _items.Sum(i => i.LineTotal);
        Total = Subtotal;
    }
}
