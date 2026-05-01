using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Dashboard.SwapImageIndex;

public record SwapImageIndexCommand(Guid ProductId, Guid ImageId, Guid StoreId, int OldIndex, int NewIndex)
    : ICommand<SwapImageIndexResponse>;

public record SwapImageIndexResponse(List<ProductImageDto> Images);

public class SwapImageIndexHandler(CatalogDbContext dbContext)
    : ICommandHandler<SwapImageIndexCommand, SwapImageIndexResponse>
{
    public async Task<SwapImageIndexResponse> Handle(SwapImageIndexCommand request, CancellationToken cancellationToken)
    {
        var productExists = await dbContext.Products
            .ForStore(request.StoreId)
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            throw new ProductNotFound("Product not found");

        var images = await dbContext.ProductImages
            .Where(i => i.ProductId == request.ProductId)
            .OrderBy(i => i.Index)
            .ToListAsync(cancellationToken);

        var sourceImage = images.SingleOrDefault(i => i.Id == request.ImageId && i.Index == request.OldIndex);
        if (sourceImage is null)
            throw new ProductImageNotFound("Image not found at the specified index");

        var targetImage = images.SingleOrDefault(i => i.Index == request.NewIndex);
        if (targetImage is null)
            throw new InvalidImageIndex($"No image exists at index {request.NewIndex}");

        // Swap
        sourceImage.UpdateIndex(request.NewIndex);
        targetImage.UpdateIndex(request.OldIndex);

        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedImages = images
            .OrderBy(i => i.Index)
            .Select(i => new ProductImageDto(i.Id, i.Url, i.IsPrimary, i.Index))
            .ToList();

        return new SwapImageIndexResponse(updatedImages);
    }
}
