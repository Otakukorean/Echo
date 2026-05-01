namespace Stores.Stores.Models;

public class Store : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public string LogoUrl { get; private set; } = null!;
    public string? CoverUrl { get; private set; }

    private Store() { }

    public static Store Create(string name, string slug, string description, string logoUrl, string? coverUrl, Guid ownerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(logoUrl);

        if (ownerId == Guid.Empty)
            throw new ArgumentException("OwnerId cannot be empty.", nameof(ownerId));

        return new Store
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = description,
            LogoUrl = logoUrl,
            CoverUrl = coverUrl,
            OwnerId = ownerId
        };
    }
    
    public void Update(string name, string slug, string description, string logoUrl, string? coverUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(logoUrl);
        
        Name = name;
        Slug = slug;
        Description = description;
        LogoUrl = logoUrl;
        CoverUrl = coverUrl;
    }
}
