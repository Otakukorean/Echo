namespace Catalog.Catalog.Features.Dashboard.SetPrimaryImage;

public class SetPrimaryImageEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{productId:guid}/images/{imageId:guid}/primary", async (
            Guid productId,
            Guid imageId,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new SetPrimaryImageCommand(productId, imageId, store.Id));
            return Results.Ok(response);
        })
        .Produces<SetPrimaryImageResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Product Images")
        .WithName("SetPrimaryImage")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
