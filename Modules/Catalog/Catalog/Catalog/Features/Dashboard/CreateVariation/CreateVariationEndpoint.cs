using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.FileStorage;

namespace Catalog.Catalog.Features.Dashboard.CreateVariation;

public class CreateVariationRequest
{
    [FromForm(Name = "value")]
    public string Value { get; set; } = default!;

    [FromForm(Name = "price")]
    public decimal Price { get; set; }

    [FromForm(Name = "active")]
    public bool Active { get; set; }

    [FromForm(Name = "color")]
    public string? Color { get; set; }

    public IFormFile? Url { get; set; }

    [FromForm(Name = "quantity")]
    public int Quantity { get; set; }
}

public class CreateVariationEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId:guid}/variations", async Task<Created<CreateVariationResponse>> (
            Guid productId,
            [AsParameters] CreateVariationRequest request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var dto = new CreateVariationDto(
                request.Value, request.Price, request.Active, request.Color, request.Quantity,
                request.Url?.OpenReadStream(),
                request.Url?.FileName,
                request.Url?.ContentType);
            var response = await sender.Send(new CreateVariationCommand(dto, productId, store.Id));
            return TypedResults.Created($"/products/{productId}/variations/{response.Variation.Id}", response);
        })
        .Produces<CreateVariationResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Variations")
        .WithName("CreateVariation")
        .DisableAntiforgery()
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
