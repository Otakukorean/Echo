using Catalog.Catalog.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Exceptions;
using Stores.Contracts.Features;

namespace Catalog.Catalog.Features.Public.GetProductById;

public class GetProductByIdEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storefront/{slug}/products/{productId:guid}", async Task<Ok<GetProductByIdResponse>> (
            string slug,
            Guid productId,
            ISender sender) =>
        {
            var storeResult = await sender.Send(new GetStoreBySlugQuery(slug));
            if (storeResult.Store is null)
                throw new NotFoundException("Store", slug);

            var response = await sender.Send(new GetProductByIdQuery(productId, storeResult.Store.StoreId));
            return TypedResults.Ok(response);
        })
        .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Storefront")
        .WithName("GetProductById")
        .AllowAnonymous();
    }
}
