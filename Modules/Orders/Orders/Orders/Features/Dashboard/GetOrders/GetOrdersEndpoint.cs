using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Pagination;

namespace Orders.Orders.Features.Dashboard.GetOrders;

public class GetOrdersEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async Task<Ok<GetOrdersResponse>> (
            ISender sender,
            IStoreContext storeContext,
            int pageIndex = 0,
            int pageSize = 10,
            string? status = null) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new GetOrdersQuery(
                store.Id,
                new PaginationRequest(pageIndex, pageSize),
                status));

            return TypedResults.Ok(response);
        })
        .Produces<GetOrdersResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Orders")
        .WithName("GetOrders")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
