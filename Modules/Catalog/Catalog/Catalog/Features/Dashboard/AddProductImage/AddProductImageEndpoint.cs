using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Catalog.Features.Dashboard.AddProductImage;

public class AddProductImageRequest
{
    public IFormFile File { get; set; } = default!;

    [FromForm(Name = "isPrimary")]
    public bool IsPrimary { get; set; }

    [FromForm(Name = "index")]
    public int Index { get; set; }
}

public class AddProductImageEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId:guid}/images", async Task<Ok<AddProductImageResponse>> (
            Guid productId,
            [AsParameters] AddProductImageRequest request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();

            var response = await sender.Send(new AddProductImageCommand(
                productId,
                store.Id,
                request.File.OpenReadStream(),
                request.File.FileName,
                request.File.ContentType,
                request.IsPrimary,
                request.Index));

            return TypedResults.Ok(response);
        })
        .Produces<AddProductImageResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Product Images")
        .WithName("AddProductImage")
        .DisableAntiforgery()
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
