using Catalog.Contracts.Dtos;
using Catalog.Contracts.Features;

namespace Catalog.Catalog.Features.Internal.GetProductsByIds;

public class GetProductsByIdsHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetProductsByIdsQuery, GetProductsByIdsResult>
{
    public async Task<GetProductsByIdsResult> Handle(GetProductsByIdsQuery query, CancellationToken cancellationToken)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .ForStore(query.StoreId)
            .Where(p => query.ProductIds.Contains(p.Id))
            .Select(p => new ProductInfoDto(p.Id, p.Name, p.Price, p.IsActive, null))
            .ToListAsync(cancellationToken);

        return new GetProductsByIdsResult(products);
    }
}
