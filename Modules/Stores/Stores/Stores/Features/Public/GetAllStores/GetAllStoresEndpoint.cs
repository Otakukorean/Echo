using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Pagination;

namespace Stores.Stores.Features.GetAllStores;


public class GetAllStoresEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stores", async ([AsParameters] PaginationRequest request, ISender sender) =>
        {
            var stores = await sender.Send(new GetAllStoresQuery(request));
            return Results.Ok(stores);
        })
        .Produces<GetAllStoresResult>(StatusCodes.Status200OK)
        .WithTags("Stores")
        .WithName("GetAllStores")
        .AllowAnonymous()
        ; 
    }
}