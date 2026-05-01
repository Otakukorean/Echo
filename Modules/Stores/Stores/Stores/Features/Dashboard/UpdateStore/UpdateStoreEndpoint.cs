using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Stores.Stores.Features.Dashboard.UpdateStore;


public record UpdateStoreRequestBody(UpdateStoreDto UpdateStoreDto);

public class UpdateStoreEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/stores", async ( [FromBody] UpdateStoreRequestBody request,ISender sender, IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new UpdateStoreRequest(request.UpdateStoreDto, store.Id));
            return Results.Ok(response);
        })
        .Produces<UpdateStoreResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Stores")
        .WithName("UpdateStore")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}