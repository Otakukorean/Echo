namespace Catalog.Catalog.Features.Dashboard.DeleteCategory;

public class DeleteCategoryEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{categoryId:guid}", async (
            Guid categoryId,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new DeleteCategoryCommand(categoryId, store.Id));
            return Results.Ok(response);
        })
        .Produces<DeleteCategoryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Categories")
        .WithName("DeleteCategory")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}
