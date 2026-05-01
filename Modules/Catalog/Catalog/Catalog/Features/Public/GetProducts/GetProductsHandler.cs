using Shared.Pagination;

namespace Catalog.Catalog.Features.Public.GetProducts;

public record GetProductsQuery(
    Guid StoreId,
    PaginationRequest Pagination,
    string? Search,
    Guid? CategoryId,
    Guid? VariationId) : IQuery<GetProductsResponse>;

public record GetProductsResponse(PaginatedResult<ProductDto> Products);

public class GetProductsHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetProductsQuery, GetProductsResponse>
{
    public async Task<GetProductsResponse> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var baseQuery = dbContext.Products
            .AsNoTracking()
            .Include(p => p.Categories)
            .ForStore(query.StoreId)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            baseQuery = baseQuery.Where(p =>
                p.Name.ToLower().Contains(search) ||
                (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        if (query.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(p =>
                p.Categories.Any(c => c.Id == query.CategoryId.Value));
        }

        if (query.VariationId.HasValue)
        {
            baseQuery = baseQuery.Where(p =>
                p.Variations.Any(v => v.Id == query.VariationId.Value));
        }

        var totalCount = await baseQuery.LongCountAsync(cancellationToken);

        var pageIndex = query.Pagination.PageIndex;
        var pageSize = query.Pagination.PageSize;

        var products = await baseQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Include(p => p.Images)
            .Include(p => p.Variations)
            .ToListAsync(cancellationToken);

        var productDtos = products.Select(p => p.ToDto()).ToList();

        return new GetProductsResponse(
            new PaginatedResult<ProductDto>(pageIndex, pageSize, totalCount, productDtos));
    }
}
