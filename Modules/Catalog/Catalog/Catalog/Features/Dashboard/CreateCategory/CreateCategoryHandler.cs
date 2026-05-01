using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Dashboard.CreateCategory;

public record CreateCategoryCommand(CreateCategoryDto Dto, Guid StoreId) : ICommand<CreateCategoryResponse>;

public record CreateCategoryResponse(CategoryDto Category);

public class CreateCategoryHandler(CatalogDbContext dbContext)
    : ICommandHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slugExists = await dbContext.Categories
            .ForStore(request.StoreId)
            .AnyAsync(c => c.Slug == request.Dto.Slug, cancellationToken);

        if (slugExists)
            throw new CategorySlugAlreadyExists("A category with this slug already exists in this store");

        var category = Category.Create(
            request.StoreId,
            request.Dto.Name,
            request.Dto.Slug,
            request.Dto.Description);

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResponse(new CategoryDto(category.Id, category.Name, category.Slug));
    }
}
