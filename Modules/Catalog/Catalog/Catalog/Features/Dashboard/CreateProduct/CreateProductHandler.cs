using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Dashboard.CreateProduct;

public record CreateProductCommand(CreateProductDto Dto, Guid StoreId) : ICommand<CreateProductResponse>;

public record CreateProductResponse(ProductDto Product);

public class CreateProductHandler(CatalogDbContext dbContext)
    : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var slugExists = await dbContext.Products
            .ForStore(request.StoreId)
            .AnyAsync(p => p.Slug == request.Dto.Slug, cancellationToken);

        if (slugExists)
            throw new ProductSlugAlreadyExists("A product with this slug already exists in this store");

        var categories = await dbContext.Categories
            .ForStore(request.StoreId)
            .Where(c => request.Dto.CategoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var product = Products.CreateProduct(
            request.StoreId,
            request.Dto.Name,
            request.Dto.Slug,
            request.Dto.Description,
            request.Dto.Price,
            request.Dto.Currency,
            request.Dto.IsActive,
            request.Dto.Sku,
            categories);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProductResponse(product.ToDto());
    }
}
