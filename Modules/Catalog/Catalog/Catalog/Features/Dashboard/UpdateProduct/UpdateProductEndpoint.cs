using Microsoft.AspNetCore.Mvc;

namespace Catalog.Catalog.Features.Dashboard.UpdateProduct;

public record UpdateProductRequestBody(UpdateProductDto UpdateProductDto);

public class UpdateProductEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{productId:guid}", async (
            Guid productId,
            [FromBody] UpdateProductRequestBody request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new UpdateProductCommand(request.UpdateProductDto, productId, store.Id));
            return Results.Ok(response);
        })
        .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Products")
        .WithName("UpdateProduct")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
