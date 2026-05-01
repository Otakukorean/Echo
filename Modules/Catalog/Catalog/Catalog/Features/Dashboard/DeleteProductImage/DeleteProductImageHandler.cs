using Catalog.Catalog.Exceptions;
using Shared.Contracts.FileStorage;

namespace Catalog.Catalog.Features.Dashboard.DeleteProductImage;

public record DeleteProductImageCommand(Guid ProductId, Guid ImageId, Guid StoreId) : ICommand<DeleteProductImageResponse>;

public record DeleteProductImageResponse(bool Success);

public class DeleteProductImageHandler(CatalogDbContext dbContext, IFileStorageService fileStorage)
    : ICommandHandler<DeleteProductImageCommand, DeleteProductImageResponse>
{
    public async Task<DeleteProductImageResponse> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await dbContext.ProductImages
            .SingleOrDefaultAsync(i => i.Id == request.ImageId && i.ProductId == request.ProductId, cancellationToken);

        if (image is null)
            throw new ProductImageNotFound("Image not found");

        // Verify product belongs to store
        var productExists = await dbContext.Products
            .ForStore(request.StoreId)
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            throw new ProductNotFound("Product not found");

        // Delete from blob storage
        await fileStorage.DeleteAsync(image.Url, cancellationToken);

        dbContext.ProductImages.Remove(image);

        // If deleted image was primary, promote the lowest-index remaining image
        if (image.IsPrimary)
        {
            var nextPrimary = await dbContext.ProductImages
                .Where(i => i.ProductId == request.ProductId && i.Id != request.ImageId)
                .OrderBy(i => i.Index)
                .FirstOrDefaultAsync(cancellationToken);

            nextPrimary?.SetAsPrimary();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteProductImageResponse(true);
    }
}
