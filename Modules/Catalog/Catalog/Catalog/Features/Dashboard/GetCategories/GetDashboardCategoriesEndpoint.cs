using Microsoft.AspNetCore.Http.HttpResults;

namespace Catalog.Catalog.Features.Dashboard.GetCategories;

public class GetDashboardCategoriesEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async Task<Ok<GetDashboardCategoriesResponse>> (
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new GetDashboardCategoriesQuery(store.Id));
            return TypedResults.Ok(response);
        })
        .Produces<GetDashboardCategoriesResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Categories")
        .WithName("GetDashboardCategories")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
