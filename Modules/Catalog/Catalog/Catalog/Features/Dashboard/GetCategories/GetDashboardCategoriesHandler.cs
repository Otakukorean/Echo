namespace Catalog.Catalog.Features.Dashboard.GetCategories;

public record GetDashboardCategoriesQuery(Guid StoreId) : IQuery<GetDashboardCategoriesResponse>;

public record GetDashboardCategoriesResponse(List<CategoryDto> Categories);

public class GetDashboardCategoriesHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetDashboardCategoriesQuery, GetDashboardCategoriesResponse>
{
    public async Task<GetDashboardCategoriesResponse> Handle(GetDashboardCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .ForStore(query.StoreId)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug))
            .ToListAsync(cancellationToken);

        return new GetDashboardCategoriesResponse(categories);
    }
}
