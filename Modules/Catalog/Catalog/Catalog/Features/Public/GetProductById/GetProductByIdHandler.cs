using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Public.GetProductById;

public record GetProductByIdQuery(Guid ProductId, Guid StoreId) : IQuery<GetProductByIdResponse>;

public record GetProductByIdResponse(ProductDto Product);

public class GetProductByIdHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResponse>
{
    public async Task<GetProductByIdResponse> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .ForStore(query.StoreId)
            .Where(p => p.IsActive)
            .SingleOrDefaultAsync(p => p.Id == query.ProductId, cancellationToken);

        if (product is null)
            throw new ProductNotFound("Product not found");

        return new GetProductByIdResponse(product.ToDto());
    }
}
