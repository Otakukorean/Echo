using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Pagination;

namespace Catalog.Catalog.Features.Dashboard.GetProducts;

public class GetDashboardProductsEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async Task<Ok<GetDashboardProductsResponse>> (
            ISender sender,
            IStoreContext storeContext,
            int pageIndex = 0,
            int pageSize = 10,
            string? search = null,
            Guid? categoryId = null,
            Guid? variationId = null) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new GetDashboardProductsQuery(
                store.Id,
                new PaginationRequest(pageIndex, pageSize),
                search,
                categoryId,
                variationId));

            return TypedResults.Ok(response);
        })
        .Produces<GetDashboardProductsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Products")
        .WithName("GetDashboardProducts")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
