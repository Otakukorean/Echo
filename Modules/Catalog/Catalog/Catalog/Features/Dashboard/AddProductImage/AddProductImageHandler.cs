using Catalog.Catalog.Exceptions;
using Shared.Contracts.FileStorage;

namespace Catalog.Catalog.Features.Dashboard.AddProductImage;

public record AddProductImageCommand(
    Guid ProductId,
    Guid StoreId,
    Stream FileStream,
    string FileName,
    string ContentType,
    bool IsPrimary,
    int Index) : ICommand<AddProductImageResponse>;

public record AddProductImageResponse(ProductImageDto Image);

public class AddProductImageHandler(CatalogDbContext dbContext, IFileStorageService fileStorage)
    : ICommandHandler<AddProductImageCommand, AddProductImageResponse>
{
    private const string ContainerName = "products";

    public async Task<AddProductImageResponse> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var productExists = await dbContext.Products
            .ForStore(request.StoreId)
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            throw new ProductNotFound("Product not found");

        // Upload file to blob storage
        var folder = $"{request.StoreId}/{request.ProductId}";
        var uploadResult = await fileStorage.UploadAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            ContainerName,
            folder,
            cancellationToken);

        // If this image is primary, unset all existing primary images for this product
        if (request.IsPrimary)
        {
            var existingPrimary = await dbContext.ProductImages
                .Where(i => i.ProductId == request.ProductId && i.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var img in existingPrimary)
                img.UnsetPrimary();
        }

        var image = ProductImage.Create(request.ProductId, uploadResult.Url, request.IsPrimary, request.Index);
        dbContext.ProductImages.Add(image);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AddProductImageResponse(
            new ProductImageDto(image.Id, image.Url, image.IsPrimary, image.Index));
    }
}
