using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Exceptions;
using Stores.Contracts.Features;

namespace Catalog.Catalog.Features.Public.GetVariations;

public class GetVariationsEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storefront/{slug}/variations", async Task<Ok<GetVariationsResponse>> (
            string slug,
            ISender sender) =>
        {
            var storeResult = await sender.Send(new GetStoreBySlugQuery(slug));
            if (storeResult.Store is null)
                throw new NotFoundException("Store", slug);

            var response = await sender.Send(new GetVariationsQuery(storeResult.Store.StoreId));
            return TypedResults.Ok(response);
        })
        .Produces<GetVariationsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Storefront")
        .WithName("GetStoreVariations")
        .AllowAnonymous();
    }
}
