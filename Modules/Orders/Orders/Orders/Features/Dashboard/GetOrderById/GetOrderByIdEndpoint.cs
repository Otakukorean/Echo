using Microsoft.AspNetCore.Http.HttpResults;

namespace Orders.Orders.Features.Dashboard.GetOrderById;

public class GetOrderByIdEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId:guid}", async Task<Ok<GetOrderByIdResponse>> (
            Guid orderId,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new GetOrderByIdQuery(orderId, store.Id));
            return TypedResults.Ok(response);
        })
        .Produces<GetOrderByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Orders")
        .WithName("GetOrderById")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
