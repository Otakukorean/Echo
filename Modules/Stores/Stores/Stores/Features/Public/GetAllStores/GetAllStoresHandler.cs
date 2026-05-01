using Shared.Pagination;

namespace Stores.Stores.Features.GetAllStores;

public record GetAllStoresQuery(PaginationRequest Request) : IQuery<GetAllStoresResult>;

public record GetAllStoresResult(PaginatedResult<StoreDto> Stores);

public class GetAllStoresHandler(StoresDbContext dbContext) : IQueryHandler<GetAllStoresQuery, GetAllStoresResult>
{
    public async Task<GetAllStoresResult> Handle(GetAllStoresQuery query, CancellationToken cancellationToken)
    {
        var pageIndex = query.Request.PageIndex;
        var pageSize = query.Request.PageSize;
        var totalCount = await dbContext.Stores.LongCountAsync(cancellationToken);
        
        var stores = await dbContext.Stores
            .OrderByDescending(x => x.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(x => new StoreDto(x.Id, x.Name, x.Slug, x.Description, x.LogoUrl, x.CoverUrl, x.CreatedAt, x.OwnerId))
            .ToListAsync(cancellationToken);
        
        return new GetAllStoresResult(new PaginatedResult<StoreDto>(pageIndex, pageSize, totalCount , stores));
    }
}