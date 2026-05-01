using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Dashboard.UpdateCategory;

public record UpdateCategoryCommand(UpdateCategoryDto Dto, Guid CategoryId, Guid StoreId) : ICommand<UpdateCategoryResponse>;

public record UpdateCategoryResponse(CategoryDto Category);

public class UpdateCategoryHandler(CatalogDbContext dbContext)
    : ICommandHandler<UpdateCategoryCommand, UpdateCategoryResponse>
{
    public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .ForStore(request.StoreId)
            .SingleOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
            throw new CategoryNotFound("Category not found");

        var slugExists = await dbContext.Categories
            .ForStore(request.StoreId)
            .AnyAsync(c => c.Slug == request.Dto.Slug && c.Id != request.CategoryId, cancellationToken);

        if (slugExists)
            throw new CategorySlugAlreadyExists("A category with this slug already exists in this store");

        category.Update(request.Dto.Name, request.Dto.Slug, request.Dto.Description);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryResponse(new CategoryDto(category.Id, category.Name, category.Slug));
    }
}
