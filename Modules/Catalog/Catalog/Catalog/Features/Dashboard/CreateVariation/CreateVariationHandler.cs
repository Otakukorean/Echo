using Catalog.Catalog.Exceptions;
using Shared.Contracts.FileStorage;

namespace Catalog.Catalog.Features.Dashboard.CreateVariation;

public record CreateVariationCommand(CreateVariationDto Dto, Guid ProductId, Guid StoreId) : ICommand<CreateVariationResponse>;

public record CreateVariationResponse(VariationDto Variation);

public class CreateVariationHandler(CatalogDbContext dbContext , IFileStorageService fileStorage)
    : ICommandHandler<CreateVariationCommand, CreateVariationResponse>
{
    private const string ContainerName = "products-vairations";

    public async Task<CreateVariationResponse> Handle(CreateVariationCommand request, CancellationToken cancellationToken)
    {
        var productExists = await dbContext.Products
            .ForStore(request.StoreId)
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            throw new ProductNotFound("Product not found");

        string? imageUrl = null;
        if (request.Dto.FileStream is not null && request.Dto.FileName is not null && request.Dto.ContentType is not null)
        {
            var folder = $"{request.StoreId}/{request.ProductId}";
            var uploadResult = await fileStorage.UploadAsync(
                request.Dto.FileStream,
                request.Dto.FileName,
                request.Dto.ContentType,
                ContainerName,
                folder,
                cancellationToken);
            imageUrl = uploadResult.Url;
        }
        
        var variation = Variation.Create(
            request.Dto.Quantity,
            request.Dto.Color,
            imageUrl,
            request.Dto.Price,
            request.Dto.Value,
            request.Dto.Active,
            request.ProductId,
            request.StoreId);

        dbContext.Variations.Add(variation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateVariationResponse(
            new VariationDto(variation.Id, variation.Value, variation.Price, variation.Active, variation.Color, variation.Url, variation.Quantity));
    }
}
