namespace Catalog.Catalog.Features.Dashboard.DeleteProductImage;

public class DeleteProductImageEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{productId:guid}/images/{imageId:guid}", async (
            Guid productId,
            Guid imageId,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new DeleteProductImageCommand(productId, imageId, store.Id));
            return Results.Ok(response);
        })
        .Produces<DeleteProductImageResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Product Images")
        .WithName("DeleteProductImage")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
