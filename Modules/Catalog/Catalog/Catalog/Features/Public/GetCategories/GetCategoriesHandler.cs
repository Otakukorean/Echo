namespace Catalog.Catalog.Features.Public.GetCategories;

public record GetCategoriesQuery(Guid StoreId) : IQuery<GetCategoriesResponse>;

public record GetCategoriesResponse(List<CategoryDto> Categories);

public class GetCategoriesHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .ForStore(query.StoreId)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug))
            .ToListAsync(cancellationToken);

        return new GetCategoriesResponse(categories);
    }
}
