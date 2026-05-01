namespace Catalog.Catalog.Models;

public class Products : Aggregate<Guid>
{
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public string? Sku { get; private set; }

    private readonly List<Category> _categories = new();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private Products() { }

    public static Products CreateProduct(Guid storeId, string name, string slug, string? description, decimal price,
        string currency, bool isActive, string? sku, IEnumerable<Category> categories)
    {
        var product = new Products
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = name,
            Slug = slug,
            Description = description,
            Price = price,
            Currency = currency,
            IsActive = isActive,
            Sku = sku
        };

        foreach (var category in categories)
            product._categories.Add(category);

        return product;
    }

    public void AddCategory(Category category)
    {
        if (_categories.Any(c => c.Id == category.Id))
            return;

        _categories.Add(category);
    }

    public void RemoveCategory(Guid categoryId)
    {
        var category = _categories.FirstOrDefault(c => c.Id == categoryId);
        if (category is not null)
            _categories.Remove(category);
    }

    public ProductImage AddImage(string url, bool isPrimary, int index)
    {
        if (isPrimary)
        {
            foreach (var img in _images)
                img.UnsetPrimary();
        }

        var image = ProductImage.Create(Id, url, isPrimary, index);
        _images.Add(image);
        return image;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null) return;

        _images.Remove(image);

        if (image.IsPrimary && _images.Count > 0)
        {
            _images.OrderBy(i => i.Index).First().SetAsPrimary();
        }
    }

    public void SetPrimaryImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null) return;

        foreach (var img in _images)
            img.UnsetPrimary();

        image.SetAsPrimary();
    }

    public void ReorderImages(IReadOnlyList<Guid> orderedImageIds)
    {
        for (var i = 0; i < orderedImageIds.Count; i++)
        {
            var image = _images.FirstOrDefault(img => img.Id == orderedImageIds[i]);
            image?.UpdateIndex(i);
        }
    }
}
