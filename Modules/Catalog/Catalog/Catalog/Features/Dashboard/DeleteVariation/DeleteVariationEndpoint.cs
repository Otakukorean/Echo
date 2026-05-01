namespace Catalog.Catalog.Features.Dashboard.DeleteVariation;

public class DeleteVariationEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{productId:guid}/variations/{variationId:guid}", async (
            Guid productId,
            Guid variationId,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new DeleteVariationCommand(variationId, productId, store.Id));
            return Results.Ok(response);
        })
        .Produces<DeleteVariationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Variations")
        .WithName("DeleteVariation")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
