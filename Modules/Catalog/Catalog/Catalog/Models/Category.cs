using Shared.Contracts.StoreScoping;

namespace Catalog.Catalog.Models;

public class Category : Entity<Guid>, IStoreScoped
{
    public Guid StoreId { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    
    private Category() { }
    
    
    public static Category Create(Guid storeId, string name, string slug, string? description)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = name,
            Slug = slug,
            Description = description
        };
    }

    public void Update(string name, string slug, string? description)
    {
        Name = name;
        Slug = slug;
        Description = description;
    }
}