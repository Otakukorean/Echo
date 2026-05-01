using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Dashboard.UpdateProduct;

public record UpdateProductCommand(UpdateProductDto Dto, Guid ProductId, Guid StoreId) : ICommand<UpdateProductResponse>;

public record UpdateProductResponse(ProductDto Product);

public class UpdateProductHandler(CatalogDbContext dbContext)
    : ICommandHandler<UpdateProductCommand, UpdateProductResponse>
{
    public async Task<UpdateProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .ForStore(request.StoreId)
            .SingleOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
            throw new ProductNotFound("Product not found");

        var slugExists = await dbContext.Products
            .ForStore(request.StoreId)
            .AnyAsync(p => p.Slug == request.Dto.Slug && p.Id != request.ProductId, cancellationToken);

        if (slugExists)
            throw new ProductSlugAlreadyExists("A product with this slug already exists in this store");

        product.Update(
            request.Dto.Name,
            request.Dto.Slug,
            request.Dto.Description,
            request.Dto.Price,
            request.Dto.Currency,
            request.Dto.IsActive,
            request.Dto.Sku);

        var newCategories = await dbContext.Categories
            .ForStore(request.StoreId)
            .Where(c => request.Dto.CategoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        product.ReplaceCategories(newCategories);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProductResponse(product.ToDto());
    }
}
