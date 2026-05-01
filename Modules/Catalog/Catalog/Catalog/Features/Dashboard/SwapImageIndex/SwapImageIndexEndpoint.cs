using Microsoft.AspNetCore.Mvc;

namespace Catalog.Catalog.Features.Dashboard.SwapImageIndex;

public record SwapImageIndexRequest(int OldIndex, int NewIndex);

public class SwapImageIndexEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{productId:guid}/images/{imageId:guid}/index", async (
            Guid productId,
            Guid imageId,
            [FromBody] SwapImageIndexRequest request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new SwapImageIndexCommand(
                productId, imageId, store.Id, request.OldIndex, request.NewIndex));
            return Results.Ok(response);
        })
        .Produces<SwapImageIndexResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Product Images")
        .WithName("SwapImageIndex")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
