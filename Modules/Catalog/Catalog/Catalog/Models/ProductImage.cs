namespace Catalog.Catalog.Models;

public class ProductImage : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = null!;
    public bool IsPrimary { get; private set; }
    public int Index { get; private set; }

    private ProductImage() { }

    public static ProductImage Create(Guid productId, string url, bool isPrimary, int index)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = url,
            IsPrimary = isPrimary,
            Index = index
        };
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    public void UnsetPrimary()
    {
        IsPrimary = false;
    }

    public void UpdateIndex(int index)
    {
        Index = index;
    }

    public void UpdateUrl(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        Url = url;
    }
}
