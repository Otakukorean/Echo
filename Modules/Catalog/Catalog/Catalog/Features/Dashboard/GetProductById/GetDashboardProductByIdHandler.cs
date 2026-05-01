using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Dashboard.GetProductById;

public record GetDashboardProductByIdQuery(Guid ProductId, Guid StoreId) : IQuery<GetDashboardProductByIdResponse>;

public record GetDashboardProductByIdResponse(ProductDto Product);

public class GetDashboardProductByIdHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetDashboardProductByIdQuery, GetDashboardProductByIdResponse>
{
    public async Task<GetDashboardProductByIdResponse> Handle(GetDashboardProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .Include(p => p.Variations)
            .ForStore(query.StoreId)
            .SingleOrDefaultAsync(p => p.Id == query.ProductId, cancellationToken);

        if (product is null)
            throw new ProductNotFound("Product not found");

        return new GetDashboardProductByIdResponse(product.ToDto());
    }
}
