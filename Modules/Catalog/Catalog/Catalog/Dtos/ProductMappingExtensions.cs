namespace Catalog.Catalog.Dtos;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Products product) =>
        new(product.Id, product.StoreId, product.Name, product.Slug, product.Description,
            product.Price, product.Currency, product.IsActive, product.Sku,
            product.Categories.Select(c => new CategoryDto(c.Id, c.Name, c.Slug)).ToList(),
            product.Images.Select(i => new ImagesDto(i.Id, i.Url, i.IsPrimary, i.Index)).ToList() ,
            product.Variations.Select(v => new VariationDto(v.Id , v.Value , v.Price , v.Active , v.Color , v.Url , v.Quantity)).ToList()
            );
}
