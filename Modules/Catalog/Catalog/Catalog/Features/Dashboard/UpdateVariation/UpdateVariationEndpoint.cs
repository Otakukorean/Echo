using Microsoft.AspNetCore.Mvc;
using Shared.FileStorage;

namespace Catalog.Catalog.Features.Dashboard.UpdateVariation;

public class UpdateVariationRequest
{
    [FromForm(Name = "value")]
    public string Value { get; set; } = default!;

    [FromForm(Name = "price")]
    public decimal Price { get; set; }

    [FromForm(Name = "active")]
    public bool Active { get; set; }

    [FromForm(Name = "color")]
    public string? Color { get; set; }

    public IFormFile? Url { get; set; } = default!;

    [FromForm(Name = "quantity")]
    public int Quantity { get; set; }
}

public class UpdateVariationEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{productId:guid}/variations/{variationId:guid}", async (
            Guid productId,
            Guid variationId,
            [AsParameters] UpdateVariationRequest request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var dto = new UpdateVariationDto(
                request.Value,
                request.Price,
                request.Active,
                request.Color,
                request.Quantity,
                request.Url?.OpenReadStream(),
                request.Url?.FileName,
                request.Url?.ContentType);
            var response = await sender.Send(new UpdateVariationCommand(dto, variationId, productId, store.Id));
            return Results.Ok(response);
        })
        .Produces<UpdateVariationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Variations")
        .WithName("UpdateVariation")
        .DisableAntiforgery()
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
