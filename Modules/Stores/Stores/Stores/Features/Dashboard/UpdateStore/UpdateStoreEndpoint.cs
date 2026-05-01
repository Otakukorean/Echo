using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Stores.Stores.Features.Dashboard.UpdateStore;

public class UpdateStoreFormRequest
{
    [FromForm(Name = "name")]
    public string Name { get; set; } = default!;

    [FromForm(Name = "slug")]
    public string Slug { get; set; } = default!;

    [FromForm(Name = "description")]
    public string Description { get; set; } = default!;

    public IFormFile? Logo { get; set; }

    public IFormFile? Cover { get; set; }
}

public class UpdateStoreEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/stores", async (
            [AsParameters] UpdateStoreFormRequest request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();

            var dto = new UpdateStoreDto(
                request.Name,
                request.Slug,
                request.Description,
                request.Logo?.OpenReadStream(),
                request.Logo?.FileName,
                request.Logo?.ContentType,
                request.Cover?.OpenReadStream(),
                request.Cover?.FileName,
                request.Cover?.ContentType);

            var response = await sender.Send(new UpdateStoreRequest(dto, store.Id));
            return Results.Ok(response);
        })
        .Produces<UpdateStoreResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Stores")
        .WithName("UpdateStore")
        .DisableAntiforgery()
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
