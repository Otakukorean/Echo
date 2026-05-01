using Microsoft.AspNetCore.Http.HttpResults;

namespace Catalog.Catalog.Features.Dashboard.GetProductById;

public class GetDashboardProductByIdEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{productId:guid}", async Task<Ok<GetDashboardProductByIdResponse>> (
            Guid productId,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new GetDashboardProductByIdQuery(productId, store.Id));
            return TypedResults.Ok(response);
        })
        .Produces<GetDashboardProductByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Products")
        .WithName("GetDashboardProductById")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
