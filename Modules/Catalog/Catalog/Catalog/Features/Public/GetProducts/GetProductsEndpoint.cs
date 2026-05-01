using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Exceptions;
using Shared.Pagination;
using Stores.Contracts.Features;

namespace Catalog.Catalog.Features.Public.GetProducts;

public class GetProductsEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storefront/{slug}/products", async Task<Ok<GetProductsResponse>> (
            string slug,
            ISender sender,
            int pageIndex = 0,
            int pageSize = 10,
            string? search = null,
            Guid? categoryId = null,
            Guid? variationId = null) =>
        {
            var storeResult = await sender.Send(new GetStoreBySlugQuery(slug));
            if (storeResult.Store is null)
                throw new NotFoundException("Store", slug);

            var response = await sender.Send(new GetProductsQuery(
                storeResult.Store.StoreId,
                new PaginationRequest(pageIndex, pageSize),
                search,
                categoryId,
                variationId));

            return TypedResults.Ok(response);
        })
        .Produces<GetProductsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Storefront")
        .WithName("GetProducts")
        .AllowAnonymous();
    }
}
