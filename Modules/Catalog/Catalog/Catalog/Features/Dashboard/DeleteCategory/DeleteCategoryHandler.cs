using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Dashboard.DeleteCategory;

public record DeleteCategoryCommand(Guid CategoryId, Guid StoreId) : ICommand<DeleteCategoryResponse>;

public record DeleteCategoryResponse(bool Success);

public class DeleteCategoryHandler(CatalogDbContext dbContext)
    : ICommandHandler<DeleteCategoryCommand, DeleteCategoryResponse>
{
    public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .ForStore(request.StoreId)
            .SingleOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
            throw new CategoryNotFound("Category not found");

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteCategoryResponse(true);
    }
}
