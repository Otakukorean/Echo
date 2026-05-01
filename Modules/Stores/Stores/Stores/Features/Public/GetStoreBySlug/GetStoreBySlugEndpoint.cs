using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Stores.Stores.Features.GetStoreBySlug;

public class GetStoreBySlugEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stores/{slug}", async (string slug,ISender sender) =>
        {
            var response = await sender.Send(new GetStoreBySlugRequest(slug));
            return Results.Ok(response);
        })
        .Produces<GetStoreBySlugResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Stores")
        .WithName("GetStoreBySlug")
        .AllowAnonymous();
    }
}