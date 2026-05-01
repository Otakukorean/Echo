using Catalog.Catalog.Exceptions;
using Shared.Contracts.FileStorage;

namespace Catalog.Catalog.Features.Dashboard.DeleteVariation;

public record DeleteVariationCommand(Guid VariationId, Guid ProductId, Guid StoreId) : ICommand<DeleteVariationResponse>;

public record DeleteVariationResponse(bool Success);

public class DeleteVariationHandler(CatalogDbContext dbContext, IFileStorageService fileStorage)
    : ICommandHandler<DeleteVariationCommand, DeleteVariationResponse>
{
    public async Task<DeleteVariationResponse> Handle(DeleteVariationCommand request, CancellationToken cancellationToken)
    {
        var productExists = await dbContext.Products
            .ForStore(request.StoreId)
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            throw new ProductNotFound("Product not found");

        var variation = await dbContext.Variations
            .SingleOrDefaultAsync(v => v.Id == request.VariationId && v.ProductId == request.ProductId, cancellationToken);

        if (variation is null)
            throw new VariationNotFound("Variation not found");

        if (!string.IsNullOrWhiteSpace(variation.Url))
            await fileStorage.DeleteAsync(variation.Url, cancellationToken);

        dbContext.Variations.Remove(variation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteVariationResponse(true);
    }
}
