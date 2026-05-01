using Catalog.Catalog.Exceptions;
using Shared.Contracts.FileStorage;

namespace Catalog.Catalog.Features.Dashboard.UpdateVariation;

public record UpdateVariationCommand(UpdateVariationDto Dto, Guid VariationId, Guid ProductId, Guid StoreId) : ICommand<UpdateVariationResponse>;

public record UpdateVariationResponse(VariationDto Variation);

public class UpdateVariationHandler(CatalogDbContext dbContext, IFileStorageService fileStorage)
    : ICommandHandler<UpdateVariationCommand, UpdateVariationResponse>
{
    private const string ContainerName = "products-vairations";

    public async Task<UpdateVariationResponse> Handle(UpdateVariationCommand request, CancellationToken cancellationToken)
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

        // Delete old image if it exists
        if (!string.IsNullOrWhiteSpace(variation.Url))
        {
            await fileStorage.DeleteAsync(variation.Url, cancellationToken);
        }

        // Upload new image
        var folder = $"{request.StoreId}/{request.ProductId}";
        var uploadResult = await fileStorage.UploadAsync(
            request.Dto.FileStream,
            request.Dto.FileName,
            request.Dto.ContentType,
            ContainerName,
            folder,
            cancellationToken);

        variation.Update(
            request.Dto.Quantity,
            request.Dto.Color,
            uploadResult.Url,
            request.Dto.Price,
            request.Dto.Value,
            request.Dto.Active);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateVariationResponse(
            new VariationDto(variation.Id, variation.Value, variation.Price, variation.Active, variation.Color, variation.Url, variation.Quantity));
    }
}
