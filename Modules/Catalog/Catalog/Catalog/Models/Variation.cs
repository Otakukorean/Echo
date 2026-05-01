using Shared.Contracts.StoreScoping;

namespace Catalog.Catalog.Models;

public class Variation : Entity<Guid>, IStoreScoped
{
    public int Quantity { get; private set; }
    public string? Color { get; private set; } = null!;
    public string? Url { get; private set; } = null!;
    public decimal Price { get; private set; }
    public string Value { get; private set; } = null!;
    public bool Active { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid StoreId { get; private set; }

    private Variation() { }

    public static Variation Create(int quantity, string? color, string? url, decimal price, string value, bool active, Guid productId, Guid storeId)
    {
        return new Variation
        {
            Id = Guid.NewGuid(),
            Quantity = quantity,
            Color = color,
            Url = url,
            Price = price,
            Value = value,
            Active = active,
            ProductId = productId,
            StoreId = storeId
        };
    }

    public void Update(int quantity, string? color, string? url, decimal price, string value, bool active)
    {
        Quantity = quantity;
        Color = color;
        Url = url;
        Price = price;
        Value = value;
        Active = active;
    }
}
