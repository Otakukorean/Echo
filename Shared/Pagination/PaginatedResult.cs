namespace Shared.Pagination;

public class PaginatedResult<TEntity>(int pageIndex , int pageSize , long totalCount , IEnumerable<TEntity> items) where TEntity : class
{
    public int PageIndex { get; } = pageIndex;
    public int PageSize { get; } = pageSize;
    public long TotalCount { get; } = totalCount;
    public IEnumerable<TEntity> Items { get; } = items;
}