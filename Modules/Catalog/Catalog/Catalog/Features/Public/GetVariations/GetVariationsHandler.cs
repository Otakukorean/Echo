namespace Catalog.Catalog.Features.Public.GetVariations;

public record GetVariationsQuery(Guid StoreId) : IQuery<GetVariationsResponse>;

public record GetVariationsResponse(List<VariationDto> Variations);

public class GetVariationsHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetVariationsQuery, GetVariationsResponse>
{
    public async Task<GetVariationsResponse> Handle(GetVariationsQuery query, CancellationToken cancellationToken)
    {
        var variations = await dbContext.Variations
            .AsNoTracking()
            .ForStore(query.StoreId)
            .Where(v => v.Active)
            .Select(v => new VariationDto(v.Id, v.Value, v.Price, v.Active, v.Color, v.Url, v.Quantity))
            .ToListAsync(cancellationToken);

        return new GetVariationsResponse(variations);
    }
}
