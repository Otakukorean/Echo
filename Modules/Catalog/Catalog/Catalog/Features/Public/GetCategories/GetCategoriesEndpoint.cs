using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Exceptions;
using Stores.Contracts.Features;

namespace Catalog.Catalog.Features.Public.GetCategories;

public class GetCategoriesEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storefront/{slug}/categories", async Task<Ok<GetCategoriesResponse>> (
            string slug,
            ISender sender) =>
        {
            var storeResult = await sender.Send(new GetStoreBySlugQuery(slug));
            if (storeResult.Store is null)
                throw new NotFoundException("Store", slug);

            var response = await sender.Send(new GetCategoriesQuery(storeResult.Store.StoreId));
            return TypedResults.Ok(response);
        })
        .Produces<GetCategoriesResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Storefront")
        .WithName("GetStoreCategories")
        .AllowAnonymous();
    }
}
