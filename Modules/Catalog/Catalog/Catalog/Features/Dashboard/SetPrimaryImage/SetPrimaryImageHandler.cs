using Catalog.Catalog.Exceptions;

namespace Catalog.Catalog.Features.Dashboard.SetPrimaryImage;

public record SetPrimaryImageCommand(Guid ProductId, Guid ImageId, Guid StoreId) : ICommand<SetPrimaryImageResponse>;

public record SetPrimaryImageResponse(ProductImageDto Image);

public class SetPrimaryImageHandler(CatalogDbContext dbContext)
    : ICommandHandler<SetPrimaryImageCommand, SetPrimaryImageResponse>
{
    public async Task<SetPrimaryImageResponse> Handle(SetPrimaryImageCommand request, CancellationToken cancellationToken)
    {
        var productExists = await dbContext.Products
            .ForStore(request.StoreId)
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            throw new ProductNotFound("Product not found");

        var images = await dbContext.ProductImages
            .Where(i => i.ProductId == request.ProductId)
            .ToListAsync(cancellationToken);

        var targetImage = images.SingleOrDefault(i => i.Id == request.ImageId);
        if (targetImage is null)
            throw new ProductImageNotFound("Image not found");

        foreach (var img in images)
            img.UnsetPrimary();

        targetImage.SetAsPrimary();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new SetPrimaryImageResponse(
            new ProductImageDto(targetImage.Id, targetImage.Url, targetImage.IsPrimary, targetImage.Index));
    }
}
