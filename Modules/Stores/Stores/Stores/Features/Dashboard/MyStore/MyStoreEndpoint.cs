using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Stores.Stores.Features.Dashboard.MyStore;

public class MyStoreEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stores/me", async Task<Ok<MyStoreResponse>> (ISender sender, IStoreContext storeContext) =>
        {
            var verifiedStore = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new MyStoreQuery(verifiedStore.Id));
            return TypedResults.Ok(response);
        })
        .Produces<MyStoreResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Stores")
        .WithName("MyStore")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
