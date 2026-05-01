using Microsoft.AspNetCore.Mvc;

namespace Orders.Orders.Features.Dashboard.UpdateOrderStatus;

public record UpdateOrderStatusRequest(string Status);

public class UpdateOrderStatusEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/orders/{orderId:guid}/status", async (
            Guid orderId,
            [FromBody] UpdateOrderStatusRequest request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new UpdateOrderStatusCommand(orderId, store.Id, request.Status));
            return Results.Ok(response);
        })
        .Produces<UpdateOrderStatusResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Orders")
        .WithName("UpdateOrderStatus")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
